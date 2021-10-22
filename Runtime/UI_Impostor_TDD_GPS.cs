using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Impostor_TDD_GPS : UI_Impostor
{
    public AtlasGPSCoordinate m_gpsCoordinate;
    public TextureSquareCoordinate m_indexCoordinate;

    private void OnValidate()
    {
        Refresh();
    }

    private void Refresh()
    {
        m_atlas.GetTextureIndex(m_gpsCoordinate, out m_indexCoordinate);
        base.SetRawImageFocus(m_indexCoordinate);
    }

    public void SetLongitude(float longitude) { m_gpsCoordinate.m_longitude = longitude; Refresh(); }
    public void SetLatitude(float latitude) { m_gpsCoordinate.m_latitude = latitude; Refresh(); }

}


public class UI_Impostor : MonoBehaviour
{
    public ImpostorAtlasGPSScriptable m_atlas;
    public RawImage m_display;

    internal void SetRawImageFocus(TextureSquareCoordinate indexCoordinate)
    {
        ImpostorAtlasGPSUtility.GetLeft2RightBot2TopRectOf(m_atlas, indexCoordinate, out Rect rectanglePosition);
       
        m_display.texture = m_atlas.m_altasInfo.m_altas;
        m_display.uvRect = rectanglePosition;
    }
}
