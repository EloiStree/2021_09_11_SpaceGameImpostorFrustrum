using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Experiment_TextureToUVMesh : MonoBehaviour
{

    public GPSTextureSplitting m_textureMetaInfo= new GPSTextureSplitting() { m_squareDimension=32, m_textureDimension=2048};
    public GPSTextureSplittingDeduction m_textureMetaInfoDeducted;
    public Transform m_whereToPutTheBuild;
    public MeshFilter m_meshFilter;
    public SkinnedMeshRenderer m_meshRenderer;

    public Mesh m_currentMesh;

    public int m_maxSquare = 10;
    public float m_maxDistanceBound = 100000;

    public Transform m_cameraPosition;
    public Camera m_cameraUsed;
    public bool m_useViewPortAngle=true;

    [System.Serializable]
    public struct FarImpostorTransformPosition {
        public Transform m_root;
        public Transform m_radiusAnchor;

        public float GetRadius()
        {
            return Vector3.Distance(m_root.position, m_radiusAnchor.position);
        }
    }

    public ImpostorPositionMono[] m_impostorsInScene;
    public FarImpostorPositionWorldPosition [ ] m_impostors;

    private void Awake()
    {
        m_job = new JobExe_ExperimentSpaceShipUV();
        m_job.Init();
        m_textureMetaInfoDeducted.SetWith(m_textureMetaInfo);
        m_job.SetTextureInfo(m_textureMetaInfo);

        m_currentMesh = new Mesh();
        m_currentMesh.name = "Space Ship Rendering";

        Vector3[] v = new Vector3[m_maxSquare * 4];
        Vector2[] uvv = new Vector2[m_maxSquare * 4];
        int[] t = new int[m_maxSquare * 6];

        m_currentMesh.bounds = new Bounds(Vector3.zero, Vector3.one * m_maxDistanceBound);

        for (int i = 0; i < m_maxSquare; i++)
        {
            int iuv = i * 4;
            int itv = i * 6;
            uvv[iuv + 0] = new Vector2(0, 0);
            uvv[iuv + 1] = new Vector2(1, 0);
            uvv[iuv + 2] = new Vector2(0, 1);
            uvv[iuv + 3] = new Vector2(1, 1);
            t[itv + 0] = iuv + 0;
            t[itv + 1] = iuv + 2;
            t[itv + 2] = iuv + 1;
            t[itv + 3] = iuv + 2;
            t[itv + 4] = iuv + 3;
            t[itv + 5] = iuv + 1;
        }
        m_currentMesh.SetVertices(v);
        m_currentMesh.SetUVs(0, uvv);
        m_currentMesh.SetTriangles(t, 0);
        m_currentMesh.SetColors(GetRandomColors(m_currentMesh.vertices.Length));
        m_meshFilter.sharedMesh = m_currentMesh;
        if (m_meshRenderer != null)
        {

            m_meshRenderer.sharedMesh = m_currentMesh;
            m_meshRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * m_maxDistanceBound);
        }

        int[] lp = new int[m_maxSquare];
        m_flushingArray = new Vector3[m_currentMesh.vertices.Length];
        m_flushingArrayUV = new Vector2[m_currentMesh.uv.Length];
    }

    private List<Color> GetRandomColors(int count)
    {
        List<Color> c = new List<Color>();
        for (int i = 0; i < count; i++)
        {
            c.Add(GetRandomColor());

        }
        return c;

    }

    private Color GetRandomColor()
    {
        return new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
    }

    private Vector3  [] m_flushingArray;
    private Vector2 [] m_flushingArrayUV;
    public JobExe_ExperimentSpaceShipUV m_job;


    void Update()
    {
        Refresh();
    }

    public int m_humm;
    public int m_humm2;
    void Refresh() {

        bool useViewport = m_useViewPortAngle && m_cameraUsed != null;
        if (m_impostorsInScene.Length != m_impostors.Length) {
            m_impostors = new FarImpostorPositionWorldPosition[m_impostorsInScene.Length];
            
        }
        for (int i = 0; i < m_impostorsInScene.Length; i++)
        {
            m_impostors[i].m_worldPosition = m_impostorsInScene[i].m_root.position;
            m_impostors[i].m_worldRotation = m_impostorsInScene[i].m_root.rotation;
            m_impostors[i].m_up = m_impostorsInScene[i].m_root.up;
            m_impostors[i].m_volumeRadius = m_impostorsInScene[i].GetRadius() ;

            Vector3 cameraLocal = Quaternion.Inverse(m_impostors[i].m_worldRotation) * (m_cameraPosition.position - m_impostors[i].m_worldPosition) ;

            if (useViewport)
            {
                m_impostors[i].m_viewPortPourcent = m_cameraUsed.WorldToViewportPoint(m_impostors[i].m_worldPosition);
                m_impostors[i].m_worldPositionViewport = m_cameraUsed.WorldToScreenPoint(m_impostors[i].m_worldPosition);
                m_impostors[i].m_worldPositionUpViewport = m_cameraUsed.WorldToScreenPoint(m_impostors[i].m_worldPosition + m_impostors[i].m_up);
            }

            //if (i == 0)
            //{
            //    Debug.DrawLine(m_cameraPosition.position, m_impostors[i].m_worldPosition, Color.green);
            //    Debug.DrawLine(cameraLocal, Vector3.zero, Color.green);
            //}


            GPSUtility.ComputeLatitudeLongitude(cameraLocal, out  m_impostors[i].m_longitude, out m_impostors[i].m_latitude);

            m_impostors[i].m_horizontalPourcent = ((180f + m_impostors[i].m_longitude) / 360f);
            m_impostors[i].m_horizontalTextureIndex = (int)(( m_impostors[i].m_horizontalPourcent) * (float)m_textureMetaInfoDeducted.m_sectionCount);
            //m_impostors[i].m_horizontalTextureIndex = m_textureMetaInfoDeducted.m_sectionCount / 2;


            m_impostors[i].m_verticalPourcent = ((90f+m_impostors[i].m_latitude)/180f);
            m_impostors[i].m_verticalTextureIndex = (int)(m_impostors[i].m_verticalPourcent * (float)m_textureMetaInfoDeducted.m_sectionCount);
            //m_impostors[i].m_verticalTextureIndex = m_textureMetaInfoDeducted.m_sectionCount/2;
        }

        
        NativeArray<FarImpostorPositionWorldPosition> temp = new NativeArray<FarImpostorPositionWorldPosition>(m_impostors, Allocator.TempJob);
        m_job.FlushPointsInfo(ref m_flushingArray, ref m_flushingArrayUV);
        m_job.SetObserverPosition(m_cameraPosition);
        m_job.SetFarImpostor(temp);
        m_job.m_useAngleFromViewPort = useViewport;
        m_humm++;
        JobHandle handle = m_job.Schedule<JobExe_ExperimentSpaceShipUV>(temp.Length, 64);
        handle.Complete();
        m_job.RecoverAndDispose(ref m_currentMesh);
        temp.Dispose();
        m_humm2++;

        m_meshFilter.sharedMesh = m_currentMesh;
        if (m_meshRenderer != null)
        {

            m_meshRenderer.sharedMesh = m_currentMesh;
            m_meshRenderer.localBounds = new Bounds(Vector3.zero, Vector3.one * m_maxDistanceBound);
        }


    }
}

[System.Serializable]
public struct FarImpostorPositionWorldPosition{

    public Vector3 m_worldPosition;
    public Quaternion m_worldRotation;
    public int m_horizontalTextureIndex;
    public int m_verticalTextureIndex;
    public float m_volumeRadius;
    public float m_longitude;
    public float m_latitude;
    public Vector3 m_up;
    public float m_horizontalPourcent;
    public float m_verticalPourcent;
    public Vector3 m_worldPositionViewport;
    public Vector3 m_worldPositionUpViewport;
    public float m_upAngle;
    public Vector3 m_viewPortPourcent;
}


[BurstCompile(CompileSynchronously = true)]
public struct JobExe_ExperimentSpaceShipUV : IJobParallelFor
{

    public int m_maxSquare;
    public NativeArray<FarImpostorPositionWorldPosition> m_impostors;
    public GPSTextureSplitting m_metaInfo;
    public GPSTextureSplittingDeduction m_metaInfoDeduction;

    public Vector3 m_observerPosition;
    public Vector3 m_observerUp;
    public Quaternion m_observerRotation;

    [NativeDisableParallelForRestriction]
    public NativeArray<Vector3> m_squareMeshVertices;
    [NativeDisableParallelForRestriction]
    public NativeArray<Vector2> m_squareMeshUV;


    public void Init()
    {

        float angle = 89.9f;
        m_pbl = Quaternion.Euler(angle, angle, 0);
        m_pbr = Quaternion.Euler(angle, -angle, 0);
        m_ptl = Quaternion.Euler(-angle, angle, 0);
        m_ptr = Quaternion.Euler(-angle, -angle, 0);

    }
    public void FlushPointsInfo(ref Vector3[] dataToFlushWith, ref Vector2[] dataToFlushWithUV)
    {
        m_squareMeshVertices = new NativeArray<Vector3>(dataToFlushWith, Allocator.TempJob);
        m_squareMeshUV = new NativeArray<Vector2>(dataToFlushWithUV, Allocator.TempJob);
    }
    public void RecoverAndDispose(ref Mesh meshToAffect)
    {
        meshToAffect.SetVertices(m_squareMeshVertices);
        meshToAffect.SetUVs(0, m_squareMeshUV);
        meshToAffect.RecalculateBounds();
        meshToAffect.RecalculateTangents();
        m_squareMeshVertices.Dispose();
        m_squareMeshUV.Dispose();
    }

    public void Execute(int index)
    {
        FarImpostorPositionWorldPosition pixel = m_impostors[index];
        SetSquare(index, ref pixel);
    }

  
    public void SetMaxSquareAllow(int maxSquare)
    {
        m_maxSquare = maxSquare;
    }
    public bool m_useAngleFromViewPort;
    public void SetSquare(int squareIndex, ref FarImpostorPositionWorldPosition pixelInfo)
    {
        Init();
        bool isVisible = true ;
        int pointIndex = squareIndex * 4;


        Vector3 direction = pixelInfo.m_worldPosition - m_observerPosition;
        //        Quaternion worldDirection = Quaternion.LookRotation(direction, Vector3.up);
        float angle = 0;
        if (m_useAngleFromViewPort)
        {
            pixelInfo.m_worldPositionViewport.z = 0;
            pixelInfo.m_worldPositionUpViewport.z = 0;
            pixelInfo.m_upAngle = Vector3.SignedAngle(pixelInfo.m_worldPositionUpViewport - pixelInfo.m_worldPositionViewport, Vector3.up, Vector3.forward);

            angle =- pixelInfo.m_upAngle;
            isVisible =  IsInViewPort(ref pixelInfo.m_viewPortPourcent,0.1f);
          
        }
        else {
            Quaternion objectLocalDirection = (Quaternion.Inverse(m_observerRotation) * pixelInfo.m_worldRotation);
            Vector3 yLocalPosition = objectLocalDirection * Vector3.up;
            yLocalPosition.z = 0;
            angle = Vector3.SignedAngle(Vector3.up, yLocalPosition, Vector3.forward);


            //if (squareIndex == 0)
            //{
            //    DisplayAxisOn(pixelInfo, pixelInfo.m_worldRotation);
            //    DisplayAxisOn(Vector3.zero, objectLocalDirection);
            //    Debug.DrawLine(Vector3.zero, yLocalPosition * 10, Color.green * 0.5f);
            //}
        }
        if (!isVisible)
            return;
        //worldDirection = worldDirection * Quaternion.Euler(0, 0, angle);
        //  objectLocalDirection = pixelInfo.m_worldRotation ;
        //worldDirection = objectLocalDirection;


        Quaternion worldDirection = m_observerRotation * Quaternion.Euler(0,180f,90f- angle) ;

        //  Quaternion objectLocalDirection = pixelInfo.m_worldRotation * Quaternion.Inverse(m_observerRotation);
     

       


        // worldDirection = worldDirection * Quaternion.Euler(0, 0, 90);

        //Rotate of camera up to object up
        //or rotate of camera up to projection of object up on camera plane
        //Vector3 relocatedObjectOnCameraRoot = Quaternion.Inverse(m_observerRotation) * (pixelInfo.m_worldPosition - m_observerPosition);
        //Vector3 relocatedObjectOnCameraUp = Quaternion.Inverse(m_observerRotation) * ((pixelInfo.m_worldPosition+ pixelInfo.m_up) - m_observerPosition);
        //Vector3 relcoatedDirection = relocatedObjectOnCameraUp - relocatedObjectOnCameraRoot;
        //relcoatedDirection.z = 0;
        //Quaternion qZ = Quaternion.LookRotation(relcoatedDirection, Vector3.up);
        //worldDirection =worldDirection * qZ;

        Vector3 vbl = new Vector3(-pixelInfo.m_volumeRadius, -pixelInfo.m_volumeRadius, 0);
        Vector3 vbr = new Vector3(-pixelInfo.m_volumeRadius, pixelInfo.m_volumeRadius, 0);
        Vector3 vtl = new Vector3(pixelInfo.m_volumeRadius, -pixelInfo.m_volumeRadius, 0);
        Vector3 vtr = new Vector3(pixelInfo.m_volumeRadius, pixelInfo.m_volumeRadius, 0);


        Vector3 position = pixelInfo.m_worldPosition;
        float mag = pixelInfo.m_volumeRadius;
        m_squareMeshVertices[pointIndex + 0] = position +  (worldDirection * vbl) * pixelInfo.m_volumeRadius;
        m_squareMeshVertices[pointIndex + 1] = position +  (worldDirection * vbr) * pixelInfo.m_volumeRadius;
        m_squareMeshVertices[pointIndex + 2] = position +  (worldDirection * vtl) * pixelInfo.m_volumeRadius;
        m_squareMeshVertices[pointIndex + 3] = position +  (worldDirection * vtr) * pixelInfo.m_volumeRadius;

        float pctSquare = m_metaInfoDeduction.m_squarePourcent;  //bot left
        //m_squareMeshUV[pointIndex + 0] = new Vector2(0,0);
        ////bot right
        //m_squareMeshUV[pointIndex + 1] = new Vector2(1,0);
        ////top left
        //m_squareMeshUV[pointIndex + 2] = new Vector2(0,1);
        ////top right
        //m_squareMeshUV[pointIndex + 3] = new Vector2(1,1);


        //bot left
        m_squareMeshUV[pointIndex + 0] = new Vector2(pctSquare * (pixelInfo.m_horizontalTextureIndex), pctSquare * (pixelInfo.m_verticalTextureIndex));
        //bot right
        m_squareMeshUV[pointIndex + 1] = new Vector2(pctSquare * (pixelInfo.m_horizontalTextureIndex + 1), pctSquare * (pixelInfo.m_verticalTextureIndex));
        //top left
        m_squareMeshUV[pointIndex + 2] = new Vector2(pctSquare * (pixelInfo.m_horizontalTextureIndex), pctSquare * (pixelInfo.m_verticalTextureIndex + 1));
        //top right
        m_squareMeshUV[pointIndex + 3] = new Vector2(pctSquare * (pixelInfo.m_horizontalTextureIndex + 1), pctSquare * (pixelInfo.m_verticalTextureIndex + 1));





    }


    private bool IsInViewPort(ref Vector3 worldPositionViewport, float additionalMarging =0.1f)
    {
        if (worldPositionViewport.x < (0f - additionalMarging))
            return false;
        if (worldPositionViewport.x > (1f + additionalMarging))
            return false;
        if (worldPositionViewport.y < (0f - additionalMarging))
            return false;
        if (worldPositionViewport.y > (1f + additionalMarging))
            return false;
        if (worldPositionViewport.z <0f)
            return false;
        return true;
     

    }

    private static void DisplayAxisOn(FarImpostorPositionWorldPosition pixelInfo, Quaternion objectLocalDirection)
    {
        DisplayAxisOn(pixelInfo.m_worldPosition, objectLocalDirection);
      
    }
    private static void DisplayAxisOn(Vector3 position, Quaternion objectLocalDirection)
    {
        Debug.DrawLine(position,
                        position + (objectLocalDirection * Vector3.forward), Color.blue, 0.05f);
        Debug.DrawLine(position,
            position + (objectLocalDirection * Vector3.up), Color.green, 0.05f);
        Debug.DrawLine(position,
            position + (objectLocalDirection * Vector3.right), Color.red, 0.05f);
    }



    public void SetTextureInfo(GPSTextureSplitting textureMetaInfo)
    {
        m_metaInfo = textureMetaInfo;
        m_metaInfoDeduction.SetWith(textureMetaInfo);
    }
   
    internal void SetFarImpostor(NativeArray<FarImpostorPositionWorldPosition> temp)
    {
        m_impostors = temp;
    }

    internal void SetObserverPosition(Transform cameraPosition)
    {
        m_observerPosition = cameraPosition.position;
        m_observerRotation = cameraPosition.rotation;
        m_observerUp = cameraPosition.up;
    }


    Quaternion m_pbl;
    Quaternion m_pbr;
    Quaternion m_ptl;
    Quaternion m_ptr;
}
