using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment_SizePixelEvaluationRange : MonoBehaviour
{

    public Camera m_useCamera;


    [TextArea(0, 10)]
    public string m_toCopyReport;

    public float m_stepDistance = 0.1f;
    public float m_maxDistanceToTest = 5000;

    public string m_nameOfDeviceSimulated = "Unnamed";
    public PipePixelRange[] m_rangeToTest = new PipePixelRange[]{
         new PipePixelRange() { m_radius = 1 },
        new PipePixelRange() { m_radius = 5 },
        new PipePixelRange() { m_radius = 10 },
        new PipePixelRange() { m_radius = 20 },
        new PipePixelRange() { m_radius = 50 },
        new PipePixelRange() { m_radius = 100 }

};

    [Header("Debug")]
    public int m_widthResolutionUsed;
    public int m_heightResolutionUsed;
    public int m_widthResolution;
    public int m_heightResolution;
    public float m_distance;

    public bool m_executeAtStart=true;
    public void Start()
    {
        if(m_executeAtStart)
         StartComputeWithCoroutine();
    }

    [ContextMenu("Compute Without Coroutine")]
    public void StartComputeWithCoroutine()
    {
        StartCoroutine(ComputeWithCoroutine());
    }
        public IEnumerator ComputeWithCoroutine() {

        float distance = 0;
        if (m_stepDistance < 0.1f)
            m_stepDistance = 0.1f;

        int horizontalPixelState=0;
        int verticalPixelState=0;
        int horizontalPixelStatePrevious=0;
        int verticalPixelStatePrevious=0;

        for (int i = 0; i < m_rangeToTest.Length; i++)
        {
            do
            {
                PixelScreenEvaluationUtility.Compute(
                    m_useCamera,
                    distance, 
                    m_rangeToTest[i].m_radius
                    , out m_widthResolutionUsed,
                    out m_heightResolutionUsed,
                    out horizontalPixelState,
                    out verticalPixelState,
                    true
                    );
                CheckFor(ref m_rangeToTest[i], distance, horizontalPixelState);

                horizontalPixelStatePrevious = horizontalPixelState;
                verticalPixelStatePrevious = verticalPixelState;
                m_widthResolution = horizontalPixelState;
                m_heightResolution = verticalPixelState;
                m_distance = distance;
                yield return new WaitForSeconds(m_waitBeweenStep);
                yield return new WaitForEndOfFrame();

                distance += m_stepDistance;
            }
            while (distance < m_maxDistanceToTest && horizontalPixelState != 1);
            distance = 0;
            UpdateReport();
        }
       
    }

    private void UpdateReport()
    {
        m_toCopyReport = "\nDevice: " + m_nameOfDeviceSimulated;
        m_toCopyReport += "\nResolution: " + m_widthResolutionUsed + "x" + m_heightResolutionUsed;
        m_toCopyReport += "\n----------------";
        for (int i = 0; i < m_rangeToTest.Length; i++)
        {
            m_toCopyReport += string.Format("\nRadius {8} > 128: {0} , 64: {1} , 32: {2} , 16: {3} , 8: {4} , 4: {5} , 2: {6} , 1: {7}  ",
                m_rangeToTest[i].m_128.m_distanceFound,
                m_rangeToTest[i].m_64.m_distanceFound,
                m_rangeToTest[i].m_32.m_distanceFound,
                m_rangeToTest[i].m_16.m_distanceFound,
                m_rangeToTest[i].m_8.m_distanceFound,
                m_rangeToTest[i].m_4.m_distanceFound,
                m_rangeToTest[i].m_2.m_distanceFound,
                m_rangeToTest[i].m_1.m_distanceFound, 
                m_rangeToTest[i].m_radius);

        }
        m_toCopyReport += "Resolution: " + m_widthResolutionUsed + "x" + m_heightResolutionUsed;
    }

    private void CheckFor(ref PipePixelRange pipe, PixelSizeType type
       , float distance, int pixelDimension)
    {
        switch (type)
        {
            case PixelSizeType._128:
                CheckFor(ref pipe, ref pipe.m_128, distance, pixelDimension);
                break;
            case PixelSizeType._64:
                CheckFor(ref pipe, ref pipe.m_64, distance, pixelDimension);
                break;
            case PixelSizeType._32:
                CheckFor(ref pipe, ref pipe.m_32, distance, pixelDimension);
                break;
            case PixelSizeType._16:
                CheckFor(ref pipe, ref pipe.m_16, distance, pixelDimension);
                break;
            case PixelSizeType._8:
                CheckFor(ref pipe, ref pipe.m_8, distance, pixelDimension);
                break;
            case PixelSizeType._4:
                CheckFor(ref pipe, ref pipe.m_4, distance, pixelDimension);
                break;
            case PixelSizeType._2:
                CheckFor(ref pipe, ref pipe.m_2, distance, pixelDimension);
                break;
            case PixelSizeType._1:
                CheckFor(ref pipe, ref pipe.m_1, distance, pixelDimension);
                break;
            default:
                break;
        }


    }
    private void CheckFor(ref PipePixelRange pipe, ref PixelSizeRange range
        , float distance, int pixelDimension)
    {
        if (range.m_distanceFound > 0f)
            return;
        int valueToSearch = (int) range.m_pixelsWanted;
        if (pixelDimension <= valueToSearch)
            range.m_distanceFound = distance;
    }

    private void CheckFor(ref PipePixelRange range , float distance, int pixelDimension)
    {
        CheckFor(ref range, PixelSizeType._128, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._64, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._32, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._16, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._8, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._4, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._2, distance, pixelDimension);
        CheckFor(ref range, PixelSizeType._1, distance, pixelDimension);

    }


    public float m_waitBeweenStep = 0.1f;

  

    private void Reset()
    {
        for (int i = 0; i < m_rangeToTest.Length; i++)
        {
            m_rangeToTest[i].Init();

        }
    }
}


[System.Serializable]
public struct PipePixelRange {

    public float m_radius;
    public PixelSizeRange m_128;
    public PixelSizeRange m_64;
    public PixelSizeRange m_32;
    public PixelSizeRange m_16;
    public PixelSizeRange m_8;
    public PixelSizeRange m_4;
    public PixelSizeRange m_2;
    public PixelSizeRange m_1;

    public void Init() {
        m_128.m_pixelsWanted= PixelSizeType._128;
        m_64.m_pixelsWanted = PixelSizeType._64; 
        m_32.m_pixelsWanted = PixelSizeType._32; 
        m_16.m_pixelsWanted = PixelSizeType._16; 
        m_8.m_pixelsWanted = PixelSizeType._8; 
        m_4.m_pixelsWanted = PixelSizeType._4;
        m_2.m_pixelsWanted = PixelSizeType._2; 
        m_1.m_pixelsWanted = PixelSizeType._1; 
    }
}
[System.Serializable]
public struct PixelSizeRange
{
    public PixelSizeType m_pixelsWanted;
    public float m_distanceFound;

}

public enum PixelSizeType : int
{
    _128 = 128, _64 = 64, _32 = 32, _16 = 16, _8 = 8, _4 = 4, _2 = 2, _1 = 1

}
public enum TextureDimensionType : int
{
    _4096 = 4096, _2048 = 2048, _1024 = 1024, _512 = 512, _256 = 256, _128 = 128, _64 = 64

}