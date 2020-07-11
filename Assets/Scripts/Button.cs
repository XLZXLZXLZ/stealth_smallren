using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Button : MonoBehaviour
{
    public int m_MaxPresses = 1;
    public TextMeshPro m_Label;
    public AudioSource m_PressedSound;
    public float m_PressDistance = 1.5f;
    public MeshRenderer m_ButtonMesh;
    public Material m_OffMaterial;
    public Material m_HintMaterial;
    public Material m_OnMaterial;
    public UnityEvent m_OnPressed;

    private bool m_Pressed;
    private int m_Presses;
    private float m_PressedTime;

    public bool IsPressable
        => TankCharacterController.Instance != null && Vector3.Distance(TankCharacterController.Instance.transform.position, transform.position) <= m_PressDistance;

    private void Update()
    {
        var material = m_Pressed ? m_OnMaterial : this.IsPressable ? m_HintMaterial : m_OffMaterial;

        if (m_ButtonMesh.sharedMaterial != material)
        {
            m_ButtonMesh.sharedMaterial = material;
        }

        if (Input.GetButtonDown("Use") && this.IsPressable)
        {
            this.Press();
        }

        if (!m_Pressed && this.IsPressable)
        {
            if (!m_Label.gameObject.activeSelf)
            {
                m_Label.gameObject.SetActive(true);
            }
        }
        else
        {
            if (m_Label.gameObject.activeSelf)
            {
                m_Label.gameObject.SetActive(false);
            }
        }

        if (m_Pressed && (m_MaxPresses == 0 || m_MaxPresses  > m_Presses) && Time.time - m_PressedTime > .4f)
        {
            m_Pressed = false;
        }
    }

    public void Press()
    {
        if (m_Pressed || (m_MaxPresses != 0 && m_Presses >= m_MaxPresses))
        {
            return;
        }

        m_Presses++;
        m_Pressed = true;
        m_PressedTime = Time.time;

        this.m_OnPressed?.Invoke();
        m_PressedSound?.Play();
    }
}
