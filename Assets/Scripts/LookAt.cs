using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public bool m_LookAtCamera;
    public GameObject m_LookAtTarget;

    void Start()
    {
        
    }

    void Update()
    {
        if (m_LookAtCamera)
        {
            m_LookAtTarget = Camera.main.gameObject;
        }

        transform.LookAt(m_LookAtTarget.transform);
    }
}
