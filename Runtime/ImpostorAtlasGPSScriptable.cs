using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImpostorGPS", menuName = "ImpostorGPS/Create", order = 1)]
public class ImpostorAtlasGPSScriptable : ScriptableObject
{
    
    public ImpostorAtlasGPS m_altasInfo;

    public void GetTextureIndex( Left2RightBot2TopPourcent texturePosition, out TextureSquareCoordinate indexCoordinate)
    {
        ImpostorAtlasGPSUtility.Get(texturePosition, m_altasInfo.GetSectionCount(), out indexCoordinate);
    }

    public void GetTextureIndex(AtlasGPSCoordinate gpsCoordinate, out TextureSquareCoordinate indexCoordinate)
    {
        ImpostorAtlasGPSUtility.Get(gpsCoordinate, m_altasInfo, out indexCoordinate);
    }

    public void OnValidate()
    {
        m_altasInfo.ComputeDeduction();
    }
}
[System.Serializable]
public struct ImpostorAtlasGPS
{
    public Texture2D m_altas;
    public Texture2D m_altasMap;
    public GPSTextureSplitting m_dimension;
    public GPSTextureSplittingDeduction m_deductedFromDimension;

    public void ComputeDeduction()
    {
        m_deductedFromDimension.SetWith(m_dimension);
    }

    public int GetSectionCount()
    {
        return m_deductedFromDimension.m_sectionCount;
    }

    public void InitIfNotYet (){
        if (!m_deductedFromDimension.WasInit()) {
            ComputeDeduction();
        }
    }
}
[System.Serializable]
public struct AtlasGPSCoordinate
{
    [Range(-90f, 90f)]
    public float m_latitude;
    [Range(-180f,180f)]
    public float m_longitude;
}

public class ImpostorAtlasGPSUtility
{


    //public void Get(ref ImpostorAtlasGPS texture, float left2Right, float bot2Top, out int indexLeft2Right, out int indexBot2top) { 

    //}
    public static void Get(Vector3 localPosition, ImpostorAtlasGPS atlas, out TextureSquareCoordinate indexCoordinate)
    {
        Get(localPosition, out AtlasGPSCoordinate gps);
        Get(gps, out Left2RightBot2TopPourcent pct);
        Get(pct, atlas.GetSectionCount(), out indexCoordinate);
    }
    public static void Get(AtlasGPSCoordinate gpsPosition, ImpostorAtlasGPS atlas, out TextureSquareCoordinate indexCoordinate)
    {
        Get(gpsPosition, out Left2RightBot2TopPourcent pct);
        Get(pct, atlas.GetSectionCount(), out indexCoordinate);
    }



    public static void Get(Vector3 localPosition, out AtlasGPSCoordinate pourcentCoordiante)
    {
        GPSUtility.ComputeLatitudeLongitude(localPosition, out pourcentCoordiante.m_longitude, out pourcentCoordiante.m_latitude);
    }
    public static void Get(AtlasGPSCoordinate gpsCoordinate, out Left2RightBot2TopPourcent pourcentCoordiante)
    {
        pourcentCoordiante.m_left2RightPercent = ((180f + gpsCoordinate.m_longitude) / 360f);
        pourcentCoordiante.m_bot2TopPercent = ((90f + gpsCoordinate.m_latitude) / 180f);

    }
    public static void GetFromGPS(float longitude, float latitude, out Left2RightBot2TopPourcent pourcentCoordiante)
    {
        pourcentCoordiante.m_left2RightPercent = ((180f + longitude) / 360f);
        pourcentCoordiante.m_bot2TopPercent = ((90f + latitude) / 180f);

    }
    public static void Get(Left2RightBot2TopPourcent texturePosition, int squareCount, out TextureSquareCoordinate indexCoordinate)
    {
        indexCoordinate.m_left2RightIndex =
            (int)((texturePosition.m_left2RightPercent) 
            * (float)squareCount);
        if (indexCoordinate.m_left2RightIndex >= squareCount)
            indexCoordinate.m_left2RightIndex = squareCount - 1;

        indexCoordinate.m_bot2TopIndex =
            (int)(texturePosition.m_bot2TopPercent
            * (float)squareCount);
        if (indexCoordinate.m_bot2TopIndex >= squareCount)
            indexCoordinate.m_bot2TopIndex = squareCount - 1;

    }

    public static void GetLeft2RightBot2TopRectOf(ImpostorAtlasGPSScriptable atlas, TextureSquareCoordinate indexCoordinate, out Rect rectanglePosition)
    {
        int count = atlas.m_altasInfo.GetSectionCount();
        GetLeft2RightBot2TopRectOf(count, indexCoordinate, out rectanglePosition);
    }
    public static void GetLeft2RightBot2TopRectOf(int squareCount, TextureSquareCoordinate indexCoordinate, out Rect rectanglePosition)
    {
        float pourcent = 1f / squareCount;
        rectanglePosition = new Rect(pourcent * indexCoordinate.m_left2RightIndex, pourcent * indexCoordinate.m_bot2TopIndex, pourcent, pourcent);
    }

    internal static void LookAtCamera(Camera targetCamera, Transform rendererRoot, float radius, ImpostorAtlasGPS atlas, out TextureSquareCoordinate coordinate)
    {
        FarImpostorPositionWorldPosition info = new FarImpostorPositionWorldPosition();

        info.m_worldPosition = rendererRoot.position;
        info.m_worldRotation = rendererRoot.rotation;
        info.m_up = rendererRoot.up;
        info.m_volumeRadius = radius;
        info.m_viewPortPourcent = targetCamera.WorldToViewportPoint(info.m_worldPosition);
        info.m_worldPositionViewport = targetCamera.WorldToScreenPoint(info.m_worldPosition);
        info.m_worldPositionUpViewport = targetCamera.WorldToScreenPoint(info.m_worldPosition
            + info.m_up);
        Vector3 cameraLocal = Quaternion.Inverse(info.m_worldRotation)
            * (targetCamera.transform.position - info.m_worldPosition);
        GPSUtility.ComputeLatitudeLongitude(cameraLocal,
            out info.m_longitude,
            out info.m_latitude);
        GetFromGPS(info.m_longitude, info.m_latitude, out Left2RightBot2TopPourcent l2rb2t);
        Get(l2rb2t, atlas.GetSectionCount(), out coordinate);

    }

        public static void GetIndexFromWorldPoint(Camera targetCamera, Transform rendererRoot, ImpostorAtlasGPS atlas,
            out TextureSquareCoordinate coordinate)
        {
            Vector3 cameraLocal = Quaternion.Inverse(rendererRoot.rotation)
                * (targetCamera.transform.position - rendererRoot.position);
            GetIndexFromLocalPoint(cameraLocal, atlas, out coordinate);
        }
        public static void GetIndexFromLocalPoint(Vector3 localPoint, ImpostorAtlasGPS atlas, out TextureSquareCoordinate coordinate)
        {
            GPSUtility.ComputeLatitudeLongitude(localPoint, out float longitude, out float latitude);
            GetFromGPS(longitude, latitude, out Left2RightBot2TopPourcent l2rb2t);
            Get(l2rb2t, atlas.GetSectionCount(), out coordinate);

        }

    public static void GetRotationToFaceCamera(Transform objectRoot, Camera targetCamera,out bool isVisible , out Quaternion computeWorldRotation)
    {
        Vector3 viewPortPourcent = targetCamera.WorldToViewportPoint(objectRoot.position);
        Vector3 worldPositionViewport = targetCamera.WorldToScreenPoint(objectRoot.position);
        Vector3 worldPositionUpViewport = targetCamera.WorldToScreenPoint(objectRoot.position + objectRoot.up);
        worldPositionViewport.z = 0;
        worldPositionUpViewport.z = 0;
        float upAngle = - Vector3.SignedAngle(worldPositionUpViewport - worldPositionViewport, Vector3.up, Vector3.forward);
    
        isVisible = IsInViewPort(ref viewPortPourcent, 0.1f);
        computeWorldRotation = targetCamera.transform.rotation * Quaternion.Euler(0, 180f,90f - upAngle);// Quaternion.Euler(0, 180f, 90f - upAngle);

    }
    private static bool IsInViewPort(ref Vector3 worldPositionViewport, float additionalMarging = 0.1f)
    {
        if (worldPositionViewport.x < (0f - additionalMarging))
            return false;
        if (worldPositionViewport.x > (1f + additionalMarging))
            return false;
        if (worldPositionViewport.y < (0f - additionalMarging))
            return false;
        if (worldPositionViewport.y > (1f + additionalMarging))
            return false;
        if (worldPositionViewport.z < 0f)
            return false;
        return true;


    }

}

