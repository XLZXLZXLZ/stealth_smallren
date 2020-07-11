using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Activator : MonoBehaviour
{
    public Material m_OnMaterial;
    public Material m_OffMaterial;
    public MeshRenderer m_Mesh;
    public UnityEvent m_OnActivated;

    private bool m_Activated;

    void Start()
    {
        
    }

    void Update()
    {
        var material = m_Activated ? m_OnMaterial : m_OffMaterial;

        if (m_Mesh.sharedMaterial != material)
        {
            m_Mesh.sharedMaterial = material;
        }
    }

    public void Activate()
    {
        m_Activated = true;

        this.m_OnActivated?.Invoke();
    }
}
