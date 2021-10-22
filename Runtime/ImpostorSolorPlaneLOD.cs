using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpostorSolorPlaneLOD : MonoBehaviour
{
    public Camera m_targetCamera;
    public ImpostorAtlasGPSScriptable m_default;
    public Renderer m_rendererToAffect;
    public Transform m_objectRoot;
    public Transform m_rendererRoot;
    [Tooltip("Usually _BaseMap or _MainTex")]
    public string m_textureName = "_MainTex";

    public TextureSquareCoordinate m_coordinate;
    public Rect m_rect;
    public bool m_isVisible;
    private void Awake()
    {
        m_rendererToAffect.material.SetTexture(m_textureName, m_default.m_altasInfo.m_altas);

    }
    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (m_targetCamera == null)
            m_targetCamera = Camera.main;
        ImpostorAtlasGPSUtility.GetIndexFromWorldPoint(m_targetCamera, m_objectRoot, m_default.m_altasInfo, out m_coordinate);
        m_rendererToAffect.material.SetTexture(m_textureName, m_default.m_altasInfo.m_altas);
        ImpostorAtlasGPSUtility.GetLeft2RightBot2TopRectOf(m_default, m_coordinate, out m_rect);
        m_rendererToAffect.material.SetTextureOffset(m_textureName, new Vector2(m_rect.x, m_rect.y));
        m_rendererToAffect.material.SetTextureScale(m_textureName, new Vector2(m_rect.width, m_rect.height));

        ImpostorAtlasGPSUtility.GetRotationToFaceCamera(m_objectRoot, m_targetCamera,out m_isVisible, out Quaternion computeRotation);
        m_rendererRoot.rotation = computeRotation*Quaternion.Euler(0,180f,90f);
    }
}

public abstract class ImpostorInScene: MonoBehaviour
{
    public abstract void GetRootTransform(out Transform tansform);
    public abstract void GetRadius(out float radius);
}
public class ImpostorInScenSize_Manual : ImpostorInScene
{

    public Transform m_root;
    public float m_radius;

    public override void GetRadius(out float radius)
    {
        radius = m_radius;
    }

    public override void GetRootTransform(out Transform tansform)
    {
        tansform = m_root;
    }
}
public class ImpostorInScenSize_Transform: ImpostorInScene
{
    public Transform m_root;
    public Transform m_volumeAnchor;

    public override void GetRadius(out float radius)
    {
        radius = Vector3.Distance(m_root.position, m_volumeAnchor.position);
    }

    public override void GetRootTransform(out Transform tansform)
    {
        tansform = m_root;
    }
}

