using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Experiment_SpaceImposteurBuilder : MonoBehaviour
{
    public Camera m_camera;
    public Color m_backgroundColor;

    public float m_moveCameraSpeed = 1;
    public Experiment_EvaluatePixelCameraCount m_pixelEvaluation;

    public int m_dimentionFor32x32=2048;
    public Texture2D m_output_32x32;
    public Texture2D m_output_32x32Map;
    public Texture2D m_currentRendering;

    public string m_name="Default";
    public string m_nameFormat="NAME_DIMENSIONTEXTURE_DIMENSIONSQUARE";
    public string m_recordPath;


    void Start()
    {
        m_camera.backgroundColor = m_backgroundColor;

        StartCoroutine(BuildImpostor());

    }

    public int m_pixelDimension = 8;
    public float m_timeBetweenWait = 0.1f;
    public float startDistance = 48;

    public RawImage m_currentRenderingDebug;
    public RawImage m_outputDebug;
    public RawImage m_outputDebugMap;


    public float m_drawDebugTime = 10;
    private IEnumerator BuildImpostor()
    {
        float speed = m_moveCameraSpeed;
        float distance = startDistance;
        bool at32Pixel = false;
        while (!at32Pixel)
        {


            yield return new WaitForSeconds(0.05f);
            distance += speed * 0.05f;
            m_camera.transform.position = new Vector3(0, 0, -distance);
            m_pixelEvaluation.FirstVersionTested();
            if (m_pixelEvaluation.m_horizontalPixel <= m_pixelDimension)
                at32Pixel = true;
        }
        float at32PixelDistance = distance;

        m_output_32x32 = new Texture2D(m_dimentionFor32x32, m_dimentionFor32x32);
        m_output_32x32Map = new Texture2D(m_dimentionFor32x32, m_dimentionFor32x32);

        int dimension = m_pixelDimension;
        int halfDimension = dimension/2;
        int count = (int)m_dimentionFor32x32 / dimension;
        int halfCount = count / 2;
        float degreeHorizontal = 180f / (count / 2f);
        float degreeVertical = 90f / (count / 2f);
        Quaternion d180 = Quaternion.Euler(0, 180f, 0f);

        copyXStart = (m_camera.pixelWidth / 2) - halfDimension;
        copyYStart = (m_camera.pixelHeight / 2) - halfDimension;
        copydimension = dimension;



        for (int h = -halfCount, iH=0; h <= halfCount; h++,iH++)
        {
            for (int v = -halfCount, iV=0; v <= halfCount; v++, iV++)
            {
                if (h == 0)
                {
                    h = 1;
                }
                if (v == 0)
                {
                    v = 1;
                }
                iHDebug = iH;
                iVDebug = iV;
                HDebug = h;
                VDebug = h;
                float hDegree = (degreeHorizontal / 2f) * Math.Sign(h) + h * degreeHorizontal;
                float vDegree = (degreeVertical / 2f) * Math.Sign(h) + v * degreeVertical;
                Quaternion q = Quaternion.Euler(vDegree, -hDegree, 0);
                Vector3 position = (q * Vector3.forward) * at32PixelDistance;
                m_camera.transform.position = position;
                m_camera.transform.rotation = q * d180;

                Debug.DrawLine(m_camera.transform.position, Vector3.zero, Color.cyan, m_drawDebugTime);

                m_camera.backgroundColor = m_backgroundColor;
                yield return new WaitForEndOfFrame();
                CutScreenPartTo(iH, dimension-iV-1, ref m_output_32x32);

                m_camera.backgroundColor = Color.green;
                yield return new WaitForEndOfFrame();
                CutScreenPartTo(iH, dimension-iV-1, ref m_output_32x32Map);

                if(m_currentRenderingDebug)
                    m_currentRenderingDebug.texture = m_currentRendering;
                if (m_outputDebug)
                    m_outputDebug.texture = m_output_32x32;
                if (m_outputDebugMap)
                    m_outputDebugMap.texture = m_output_32x32Map;

                //yield return new WaitWhile(()=>(copyRenderTexutre==false));
                yield return new WaitForSeconds(m_timeBetweenWait);
            }
        }
        ConvertToBlackWhiteMap();
        SaveAsTexture();

    }

    private void ConvertToBlackWhiteMap()
    {
        Color[] c = m_output_32x32Map.GetPixels();
        Color[] cr = m_output_32x32.GetPixels();
        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == Color.green)
            {
                c[i] = Color.black;
                cr[i] = new Color(cr[i].r, cr[i].g, cr[i].b, 0);
            }
            else {

                c[i] = Color.white;
                cr[i] = new Color(cr[i].r, cr[i].g, cr[i].b, 1f);

            }
        }
        m_output_32x32Map.SetPixels(c);
        m_output_32x32Map.Apply();
        m_output_32x32.SetPixels(cr);
        m_output_32x32.Apply();
    }

    private void SaveAsTexture()
    {
        string name = m_nameFormat.Replace("NAME", m_name).Replace("DIMENSIONTEXTURE", "" + m_dimentionFor32x32).Replace("DIMENSIONSQUARE", "" + m_pixelDimension); //NAME_DIMENSIONTEXTURE_DIMENSIONSQUARE
        string nameWarpped = "/" + name + ".png";
        string path = m_recordPath;
        string pathPlusName = path + nameWarpped;

        try
        {

            File.WriteAllBytes(pathPlusName, new byte[0]);
        }
        catch (Exception) {
            Debug.LogWarning("Impossible to write path given");
            path = Directory.GetCurrentDirectory()+"/Assets/CreatedTexture";
            Directory.CreateDirectory(path);
        }


        pathPlusName = path + nameWarpped;
        byte[] b = ImageConversion.EncodeToPNG(m_output_32x32);
        File.WriteAllBytes(pathPlusName, b); 

        nameWarpped = "/" + name + "map.png";
        pathPlusName = path + nameWarpped;
        b = ImageConversion.EncodeToPNG(m_output_32x32Map);
        File.WriteAllBytes(pathPlusName, b);



    }

    private void CutScreenPartTo(int iH, int iV, ref Texture2D copyIn)
    {
        m_currentRendering = toTexture2D(m_camera.activeTexture, copyXStart, copyYStart, copydimension, copydimension);
        Color[] c = m_currentRendering.GetPixels();
        int largeStartX = iH * copydimension;
        int largeStartY = iV * copydimension;
        int largeX;
        int largeY;
        for (int i = 0; i < c.Length; i++)
        {
            Color cc = c[i];
            largeX = largeStartX + (i % copydimension);
            largeY = largeStartY + (int)(i / copydimension);
            copyIn.SetPixel(largeX, largeY, cc);

        }
        copyIn.Apply();
    }

    private int iHDebug, iVDebug;
    private int HDebug, VDebug;
    private int copyXStart, copyYStart, copydimension;



    Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
    Texture2D toTexture2D(RenderTexture rTex, int pxStart, int pxStop, int pxWidth, int pxHeight)
    {
        // ReadPixels looks at the active RenderTexture.
        RenderTexture.active = rTex;
        Texture2D tex = new Texture2D(pxWidth, pxHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(pxStart, pxStop, pxWidth, pxHeight), 0, 0);
        tex.Apply();
        return tex;
    }
}
