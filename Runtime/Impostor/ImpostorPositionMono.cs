using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpostorPositionMono : MonoBehaviour
{
    public Transform m_root;
    public Transform m_volumeAnchor;

    public float GetRadius()
    {
        return Vector3.Distance(m_root.position, m_volumeAnchor.position);
    }
}
