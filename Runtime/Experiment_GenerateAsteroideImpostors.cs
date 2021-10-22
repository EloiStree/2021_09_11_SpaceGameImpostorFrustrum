using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment_GenerateAsteroideImpostors : MonoBehaviour
{
    public GameObject m_objectToDuplicated;
    public Transform m_zoneToPop;
    public Transform m_parent;
    public int m_wantedCount = 1000;
    public float m_radius = 200;

    public float m_scaleMin = 0.5f;
    public float m_scaleMax = 10f;


    [ContextMenu("Make them pop")]
    public void Pop()
    {
        for (int i = 0; i < m_wantedCount; i++)
        {
            GameObject g = GameObject.Instantiate(m_objectToDuplicated, m_parent);
            g.transform.parent = m_zoneToPop;
            g.transform.localPosition = new Vector3(Random.Range(-m_radius, m_radius), Random.Range(-m_radius, m_radius), Random.Range(-m_radius, m_radius));
            g.transform.rotation = m_zoneToPop.rotation;
            g.transform.Rotate(Random.Range(0f, 180f), Random.Range(0f, 180f), Random.Range(0f, 180f), Space.Self);
            g.transform.localScale= new Vector3(
                Random.Range(m_scaleMin, m_scaleMax), Random.Range(m_scaleMin, m_scaleMax), Random.Range(m_scaleMin, m_scaleMax));
            m_zoneToPop.transform.parent = m_parent;
        }
    }
}
