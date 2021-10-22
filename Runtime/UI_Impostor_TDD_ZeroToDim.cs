using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Impostor_TDD_ZeroToDim : UI_Impostor
{
    public Left2RightBot2TopPourcent m_texturePosition;
    public TextureSquareCoordinate m_indexCoordinate;

    private void OnValidate()
    {
        Refresh();
    }

    private void Refresh()
    {
        m_atlas.GetTextureIndex(m_texturePosition, out m_indexCoordinate);
        base.SetRawImageFocus(m_indexCoordinate);
    }

    public void SetHorizontal(float horizontal) { m_texturePosition.m_left2RightPercent = horizontal; Refresh(); }
    public void SetVertical(float vertical) { m_texturePosition.m_bot2TopPercent = vertical; Refresh(); }
}

[System.Serializable]
public struct Left2RightBot2TopPourcent
{
    [Range(0, 1f)]
    public float m_left2RightPercent;
    [Range(0, 1f)]
    public float m_bot2TopPercent;
}

[System.Serializable]
public struct TextureSquareCoordinate
{
    public int m_left2RightIndex;
    public int m_bot2TopIndex;
}
