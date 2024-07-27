using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment_EvaluatePixelCameraCount : MonoBehaviour
{
    public Camera m_targetCamera;

    public int m_cameraWidthResolution;
    public int m_cameraHeightResolution;
    public long m_pixelInScreen;

    public Transform m_targetSphere;
    public Transform m_targetBorder;

    public int m_horizontalPixel;
    public int m_verticalPixel;
    public long m_pixelUsed;
    public double m_areaUsedAsPourcent;
    public float m_cameraDistance;


    public float m_imposteurSectionVerticalOn2048;
    public float m_imposteurSectionHorizontalOn2048;

   
    public void FirstVersionTested()
    {
        m_cameraWidthResolution = m_targetCamera.pixelWidth;
        m_cameraHeightResolution = m_targetCamera.pixelHeight;
        m_pixelInScreen = m_cameraWidthResolution * m_cameraHeightResolution;
        float radius = Vector3.Distance(m_targetSphere.position, m_targetBorder.position);
        Vector3 center = m_targetSphere.position;
        Vector3 up = center + m_targetCamera.transform.up * radius;
        Vector3 right = center + m_targetCamera.transform.right * radius;

        Debug.DrawLine(center, up, Color.green, Time.deltaTime * 2);
        Debug.DrawLine(center, right, Color.red, Time.deltaTime * 2);

        pixelCenterPosition = m_targetCamera.WorldToScreenPoint(center);
        Vector2 pRight = m_targetCamera.WorldToScreenPoint(right);
        Vector2 pUp = m_targetCamera.WorldToScreenPoint(up);

        m_horizontalPixel = (int)Mathf.Abs(((pixelCenterPosition.x - pRight.x) * 2f));
        m_verticalPixel = (int)Mathf.Abs(((pixelCenterPosition.y - pUp.y) * 2f));
        m_pixelUsed = m_horizontalPixel * m_verticalPixel;
        m_areaUsedAsPourcent = (double)m_pixelUsed / (double)m_pixelInScreen;
        m_cameraDistance = Vector3.Distance(m_targetSphere.position, m_targetCamera.transform.position);

        m_imposteurSectionHorizontalOn2048 = 2048 / m_horizontalPixel;
        m_imposteurSectionVerticalOn2048 = 2048 / m_verticalPixel;
    }

    public Vector2 pixelCenterPosition;

    void Update()
    {
        FirstVersionTested();

    }
}


public class PixelScreenEvaluationUtility {


    public static void Compute(Camera cameraToUse,

        float distanceToTest,
        float targetRadius,
        out int cameraWidthResolution,
        out int cameraHeightResolution,
        out int horizontalPixel,
        out int verticalPixel,
        bool useDebugDraw=true
        )
    {
        Vector3 center = cameraToUse.transform.position + cameraToUse.transform.forward * distanceToTest;
        Vector3 up = center + cameraToUse.transform.up * targetRadius;
        Vector3 right = center + cameraToUse.transform.right * targetRadius;


        cameraWidthResolution = cameraToUse.pixelWidth;
        cameraHeightResolution = cameraToUse.pixelHeight;

        if (useDebugDraw) { 
            Debug.DrawLine(cameraToUse.transform.position, center, Color.white, Time.deltaTime * 2);
            Debug.DrawLine(center, up, Color.green, Time.deltaTime * 2);
            Debug.DrawLine(center, right, Color.red, Time.deltaTime * 2);
        }

        Vector2 pixelCenterPosition = cameraToUse.WorldToScreenPoint(center);
        Vector2 pRight = cameraToUse.WorldToScreenPoint(right);
        Vector2 pUp = cameraToUse.WorldToScreenPoint(up);

        horizontalPixel = (int) Mathf.Abs(((pixelCenterPosition.x - pRight.x) * 2f));
        verticalPixel = (int) Mathf.Abs(((pixelCenterPosition.y - pUp.y) * 2f));
    }

}