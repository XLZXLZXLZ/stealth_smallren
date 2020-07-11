using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelSelectButton : MonoBehaviour
{
    public int m_Level;
    public Material m_OnMaterial;
    public Material m_HoveredMaterial;
    public Material m_OffMaterial;
    public Material m_LockedMaterial;
    public TextMeshPro m_Text;
    public MeshRenderer m_Mesh;
    public AudioSource m_SelectSound;

    private bool m_Hovered;
    private bool m_Locked;

    public bool IsSelected
        => GameManager.Instance.m_SelectedLevel == m_Level;

    void Start()
    {
        
    }

    void Update()
    {
        if (!this.IsSelected)
        {
            this.CheckHover();
        }

        var material = this.IsSelected ? m_OnMaterial : m_Hovered ? m_HoveredMaterial : m_OffMaterial;
        m_Locked = !GameManager.Instance.LevelUnlocked(m_Level);

        if (m_Locked)
        {
            material = m_LockedMaterial;
        }

        if (m_Mesh.sharedMaterial != material)
        {
            m_Mesh.sharedMaterial = material;
        }

        if (!this.IsSelected && m_Hovered && !m_Locked)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                this.Select();
            }
        }

        if (m_Text != null)
        {
            if (m_Level >= LevelManager.Instance.m_Levels.Length)
            {
                m_Text.text = "NULL Level";
            }
            else
            {
                var level = LevelManager.Instance.m_Levels[m_Level];

                if (level != null)
                {
                    m_Text.text = level.m_Name;
                }
            }
        }
    }

    private void CheckHover()
    {
        m_Hovered = false;

        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            var objectHit = hit.transform;

            m_Hovered = objectHit == transform;
        }
    }

    private void Select()
    {
        m_SelectSound?.Play();
        GameManager.Instance.SelectLevel(m_Level);
    }
}
