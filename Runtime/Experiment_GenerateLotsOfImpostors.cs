using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Experiment_GenerateLotsOfImpostors : MonoBehaviour
{
    public GameObject m_objectToDuplicated;
    public Transform m_zoneToPop;
    public Transform m_parent;
    public int m_wantedCount=1000;
    public float m_radius=200;


    [ContextMenu("Make them pop")]
    public void Pop()
    {
        for (int i = 0; i < m_wantedCount; i++)
        {
            GameObject g = GameObject.Instantiate(m_objectToDuplicated, m_parent);
            g.transform.parent = m_zoneToPop;
            g.transform.localPosition = new Vector3(Random.Range(-m_radius, m_radius), 0, Random.Range(-m_radius, m_radius));
            g.transform.rotation = m_zoneToPop.rotation;
            g.transform.Rotate(0, Random.Range(0f, 180f), 0, Space.Self);
            m_zoneToPop.transform.parent = m_parent;
        }
    }

}
