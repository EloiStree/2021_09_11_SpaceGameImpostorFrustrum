using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Experiment_PlaySpaceShipImpostor : MonoBehaviour
{
    public Camera m_pointOfView;
    public Transform m_spaceshipOrientation;
    public Renderer m_quadToRotate;
    public GPSTextureSplitting m_textureInfo = new GPSTextureSplitting() { m_squareDimension=32, m_textureDimension=2048};
    public GPSTextureSplittingDeduction m_textureDeductedInfo;
    public GPSTextureSection m_cameraPosition;

    public float m_longitude;
    public float m_latitude;
    public float m_horizontalPourcent;
    public float m_verticalPourcent;
    public int m_iH;
    public int m_iV;

    public Texture2D m_sourceAtlas;
    public Texture2D m_extracted;

    

    private void Start()
    {
        m_textureDeductedInfo.SetWith(m_textureInfo);
        m_extracted = new Texture2D(m_textureInfo.m_squareDimension, m_textureInfo.m_squareDimension);
        InvokeRepeating("Refresh", 0, 0.1f);
    }

    //void Update()
    //{
    //    Refresh();

    //}

    private void Refresh()
    {
        m_quadToRotate.transform.rotation = Quaternion.LookRotation(m_pointOfView.transform.position - m_quadToRotate.transform.position, m_pointOfView.transform.up) ;

        m_textureDeductedInfo.SetWith(m_textureInfo);
        GuestUVWith(m_pointOfView.transform.position, out m_cameraPosition.m_longitude, out m_cameraPosition.m_latitude,
            out m_cameraPosition.m_texturePourcentHorizontal, out m_cameraPosition.m_texturePourcentVertical);


        m_cameraPosition.m_indexHorizontal = m_cameraPosition.m_longitude / m_textureDeductedInfo.m_sectionAngleHorizontal;
        m_cameraPosition.m_indexVertical = m_cameraPosition.m_latitude / m_textureDeductedInfo.m_sectionAngleVertical;

        if (m_cameraPosition.m_indexHorizontal >= 0)
            m_cameraPosition.m_indexHorizontal = 1 + (int)m_cameraPosition.m_indexHorizontal;

        if (m_cameraPosition.m_indexHorizontal < 0)
            m_cameraPosition.m_indexHorizontal = -1 + (int)m_cameraPosition.m_indexHorizontal;

        if (m_cameraPosition.m_indexVertical >= 0)
            m_cameraPosition.m_indexVertical = 1 + (int)m_cameraPosition.m_indexVertical;

        if (m_cameraPosition.m_indexVertical < 0)
            m_cameraPosition.m_indexVertical = -1 + (int)m_cameraPosition.m_indexVertical;

        m_cameraPosition.m_textureIndexHorizontal =(int) (180f + m_cameraPosition.m_longitude) / m_textureDeductedInfo.m_sectionAngleHalfHorizontal;
        m_cameraPosition.m_textureIndexVertical = (int) (90f + m_cameraPosition.m_latitude) / m_textureDeductedInfo.m_sectionAngleVertical;
       
        //if (m_cameraPosition.m_textureIndexHorizontal > m_textureDeductedInfo.m_sectionCountHalf)
        //{
        //    m_cameraPosition.m_textureIndexHorizontal -= 1;
        //}
        //if (m_cameraPosition.m_textureIndexVertical > m_textureDeductedInfo.m_sectionCountHalf)
        //{
        //    m_cameraPosition.m_textureIndexVertical -= 1;
        //}

        //If texutreIndex HOrizontal== middle   >> add one


        CutScreenPartTo(
            (int)m_cameraPosition.m_textureIndexHorizontal,
            (int)m_cameraPosition.m_textureIndexVertical,
            ref m_sourceAtlas, ref m_extracted);
        m_quadToRotate.sharedMaterial.SetTexture("_BaseMap", m_extracted);
    }


    private void CutScreenPartTo(int iH, int iV,ref Texture2D source, ref Texture2D copyIn)
    {

        if (source != null && copyIn != null) {
            Color [] c = copyIn.GetPixels();
            int largeStartX = iH * m_textureInfo.m_squareDimension;
            int largeStartY = iV * m_textureInfo.m_squareDimension;
            int largeX;
            int largeY;
            for (int i = 0; i < c.Length; i++)
            {
                largeX = largeStartX + (i % m_textureInfo.m_squareDimension);
                largeY = largeStartY + (int)(i / m_textureInfo.m_squareDimension);
                Color cc = source.GetPixel(largeX, largeY);
                c[i] = cc;

            }
            copyIn.SetPixels(c);
            copyIn.Apply();
        }
    }



    private void GuestUVWith(Vector3 positionRelative,
        out float longitude, out float latitude,
        out float horizontalPourcent,
        out float verticalPourcent)
    {
        longitude = 0;
        latitude = 0;
        horizontalPourcent = 0;
        verticalPourcent = 0;
        if (positionRelative == Vector3.zero) {
            return;
        }

            if (positionRelative.x >= 0 && positionRelative.z >= 0)
            {
                longitude = Mathf.Atan(positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
            }
            else if (positionRelative.x > 0 && positionRelative.z < 0)
            {
                longitude = 90f + Mathf.Atan(-positionRelative.z / positionRelative.x) * Mathf.Rad2Deg;
            }
            else if (positionRelative.x < 0 && positionRelative.z >= 0)
            {
                longitude = -Mathf.Atan(-positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
            }
            else if (positionRelative.x < 0f && positionRelative.z < 0)
            {
                longitude = -90 + -Mathf.Atan(-positionRelative.z / -positionRelative.x) * Mathf.Rad2Deg;
            }


            horizontalPourcent = ((180f + longitude) / 360);


            float z = Mathf.Abs(positionRelative.z);
            if (positionRelative.y >= 0f)
            {
                latitude = Mathf.Atan(positionRelative.y / z) * Mathf.Rad2Deg;
            }
            else if (positionRelative.y < 0f)
            {
                latitude = -Mathf.Atan(-positionRelative.y / z) * Mathf.Rad2Deg;
            }

            float correctedLatitude = latitude;

            if (positionRelative.z < 0) {
                if (positionRelative.y < 0) {

                    correctedLatitude = -90 - (90f - Math.Abs(latitude));
                }
                if (positionRelative.y >= 0)
                {

                    correctedLatitude = 90f + (90f - latitude);
                } 
            }

            verticalPourcent = ((180f + correctedLatitude) / 360f);
        
    }
    private void OnValidate()
    {

        Refresh();
    }
}
[System.Serializable]
public struct GPSTextureSplitting
{
    public int m_textureDimension;
    public int m_squareDimension;
}
[System.Serializable]
public struct GPSTextureSplittingDeduction
{
    public void SetWith(GPSTextureSplitting textureInfo)
    {
        m_squareDimensionPx = textureInfo.m_squareDimension;
        m_squareHalfDimensionPx = textureInfo.m_squareDimension / 2;
        m_sectionCount = textureInfo.m_textureDimension / textureInfo.m_squareDimension;
        m_sectionCountHalf = m_sectionCount / 2;

        m_sectionAngleHorizontal = 360f / (float)m_sectionCount;
        m_sectionAngleHalfHorizontal = m_sectionAngleHorizontal / 2f;
        m_sectionAngleVertical = 180f / (float)m_sectionCount;
        m_sectionAngleHalfVertical = m_sectionAngleVertical / 2f;

        m_squarePourcent = 1f/ (float)m_sectionCount;
        m_squarePourcentHalf = m_squarePourcent / 2f;
        m_wasCompted = true;
    }
    public bool m_wasCompted;
    public float m_sectionAngleHorizontal;
    public float m_sectionAngleHalfHorizontal;
    public float m_sectionAngleVertical;
    public float m_sectionAngleHalfVertical;

    public int m_sectionCount;
    public int m_sectionCountHalf;
    public int m_squareDimensionPx;
    public int m_squareHalfDimensionPx;

    public float m_squarePourcent;
    public float m_squarePourcentHalf;

    public bool WasInit() {
        return m_wasCompted;
    }

}
[System.Serializable]
public struct GPSTextureSection
{
    public float m_longitude;
    public float m_latitude;
    public float m_texturePourcentHorizontal;
    public float m_texturePourcentVertical;


    public float m_indexHorizontal;
    public float m_indexVertical;
    public float m_textureIndexHorizontal;
    public float m_textureIndexVertical;
}


public class GPSUtility {


    public static void ObsoleteComputeLatitudeLongitudeWithATan(Vector3 positionRelative,
           out float longitude, out float latitude)
    {
        longitude = 0;
        latitude = 0;
        if (positionRelative == Vector3.zero)
        {
            return;
        }

        if (positionRelative.x >= 0 && positionRelative.z >= 0)
        {
            longitude = Mathf.Atan(positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x > 0 && positionRelative.z < 0)
        {
            longitude = 90f + Mathf.Atan(-positionRelative.z / positionRelative.x) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x < 0 && positionRelative.z >= 0)
        {
            longitude = -Mathf.Atan(-positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x < 0f && positionRelative.z < 0)
        {
            longitude = -90 + -Mathf.Atan(-positionRelative.z / -positionRelative.x) * Mathf.Rad2Deg;
        }




        float z = Mathf.Abs(positionRelative.z);
        if (positionRelative.y >= 0f)
        {
            latitude = Mathf.Atan(positionRelative.y / z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.y < 0f)
        {
            latitude = -Mathf.Atan(-positionRelative.y / z) * Mathf.Rad2Deg;
        }
    }
    public static void ComputeLatitudeLongitude(Vector3 positionRelative,
          out float longitude, out float latitude)
    {

        float y = positionRelative.z;
        float x = positionRelative.x;
        float hypo = (float) Math.Sqrt((x * x) + (y * y));
        float switcher = Mathf.Cos(45f) * hypo;

        longitude = 0;
        latitude = 0;
        if (positionRelative == Vector3.zero)
        {
            return;
        }

        // Clock

        if (x >= 0f && y >= 0f)
        {
            if (y > switcher)
            {
                longitude = Mathf.Acos(y / hypo) * Mathf.Rad2Deg;
            }
            else {
                longitude = 90f-(Mathf.Acos(x / hypo) * Mathf.Rad2Deg);
            }

        }
        else if (x > 0f && y< 0f)
        {
            if (-y < switcher)
            {
                longitude = 90f + (Mathf.Acos(x/ hypo) * Mathf.Rad2Deg);
            }
            else
            {
                longitude = 180f - (Mathf.Acos(-y / hypo) * Mathf.Rad2Deg);
            }

        }
        ////////////
        ///
        else if (x < 0f && y>= 0f)
        {
            if (y > switcher)
            {
                longitude =- Mathf.Acos(y / hypo) * Mathf.Rad2Deg;
            }
            else
            {
                longitude = - 90f + (Mathf.Acos(-x/ hypo) * Mathf.Rad2Deg);
            }
        }
        else if (x< 0f && y < 0f)
        {

            if (-y < switcher)
            {
                longitude = -90f - (Mathf.Acos(-x / hypo) * Mathf.Rad2Deg);
            }
            else
            {
                longitude = -180f + (Mathf.Acos(-y / hypo) * Mathf.Rad2Deg);
            }
        }

        longitude *= -1f;




        positionRelative =Quaternion.Euler(0, longitude, 0) * positionRelative;
       // Debug.DrawLine(Vector3.zero, positionRelative);

        y = positionRelative.y;
        x = positionRelative.z;


        hypo = (float)Math.Sqrt((x * x) + (y * y));
        switcher = Mathf.Cos(45f) * hypo;

     


        if (y >= 0f )
        {
            if (y > switcher)
            {
                latitude = 90f-( Mathf.Acos(y / hypo) * Mathf.Rad2Deg);
            }
            else
            {
                latitude =  (Mathf.Acos(x / hypo) * Mathf.Rad2Deg);
            }

        }
        else if (y < 0f )
        {
            if (-y > switcher)
            {
                latitude = -90 + Mathf.Acos(-y / hypo) * Mathf.Rad2Deg;
            }
            else
            {
                latitude = - (Mathf.Acos(x / hypo) * Mathf.Rad2Deg);
            }
        }
       

    }


    public static void ObsoleteGuestUVWith(Vector3 positionRelative,
           out float longitude, out float latitude,
           out float horizontalPourcent,
           out float verticalPourcent)
    {
        longitude = 0;
        latitude = 0;
        horizontalPourcent = 0;
        verticalPourcent = 0;
        if (positionRelative == Vector3.zero)
        {
            return;
        }

        if (positionRelative.x >= 0 && positionRelative.z >= 0)
        {
            longitude = Mathf.Atan(positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x > 0 && positionRelative.z < 0)
        {
            longitude = 90f + Mathf.Atan(-positionRelative.z / positionRelative.x) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x < 0 && positionRelative.z >= 0)
        {
            longitude = -Mathf.Atan(-positionRelative.x / positionRelative.z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.x < 0f && positionRelative.z < 0)
        {
            longitude = -90 + -Mathf.Atan(-positionRelative.z / -positionRelative.x) * Mathf.Rad2Deg;
        }


        horizontalPourcent = ((180f + longitude) / 360);


        float z = Mathf.Abs(positionRelative.z);
        if (positionRelative.y >= 0f)
        {
            latitude = Mathf.Atan(positionRelative.y / z) * Mathf.Rad2Deg;
        }
        else if (positionRelative.y < 0f)
        {
            latitude = -Mathf.Atan(-positionRelative.y / z) * Mathf.Rad2Deg;
        }

        float correctedLatitude = latitude;

        if (positionRelative.z < 0)
        {
            if (positionRelative.y < 0)
            {

                correctedLatitude = -90f - (90f - Math.Abs(latitude));
            }
            if (positionRelative.y >= 0)
            {

                correctedLatitude = 90f + (90f - latitude);
            }
        }

        verticalPourcent = ((180f + correctedLatitude) / 360f);

    }
}