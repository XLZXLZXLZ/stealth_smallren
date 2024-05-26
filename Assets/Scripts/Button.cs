using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Button : MonoBehaviour
{
    public static Button HoveredButton;

    public int m_MaxPresses = 1;
    public float m_PressDistance = 1.5f;
    public TextMeshPro m_Label;
    public AudioSource m_PressedSound;
    public GameObject m_ButtonMoveObject;
    public Vector3 m_ButtonMove = new Vector3(0f, 1f, 0f);
    public MeshRenderer m_ButtonMesh;
    public Material m_OffMaterial;
    public Material m_HintMaterial;
    public Material m_OnMaterial;
    public UnityEvent m_OnPressed;
    public Light m_light;
    public Color onColor;

    private bool m_Pressed;
    private int m_Presses;
    private float m_PressedTime;
    private Vector3 m_OriginalMovePosition;
    private Vector3 m_CurrentMovePosition;

    public bool IsPressable
        => TankCharacterController.Instance != null && this.GetPlayerDistance() <= m_PressDistance;

    public bool IsPressableClosest
        => this.IsPressable && HoveredButton == this;

    private void Start()
    {
        if (m_ButtonMoveObject != null)
        {
            m_OriginalMovePosition = m_ButtonMoveObject.transform.localPosition;
            m_CurrentMovePosition = m_OriginalMovePosition;
        }
    }

    private void Update()
    {
        if (this.IsPressable)
        {
            if (HoveredButton == null || this.GetPlayerDistance() < HoveredButton.GetPlayerDistance())
            {
                HoveredButton = this;
            }
        }

        var material = m_Pressed ? m_OnMaterial : this.IsPressableClosest ? m_HintMaterial : m_OffMaterial;

        if (m_ButtonMesh.sharedMaterial != material)
        {
            m_ButtonMesh.sharedMaterial = material;
        }

        if (Input.GetButtonDown("Use") && this.IsPressableClosest)
        {
            this.Press();
        }

        if (!m_Pressed && this.IsPressableClosest)
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

        if (m_ButtonMoveObject != null)
        {
            var targetMovePosition = m_Pressed ? m_OriginalMovePosition + m_ButtonMove : m_OriginalMovePosition;
            m_CurrentMovePosition += (targetMovePosition - m_CurrentMovePosition) * Time.deltaTime * 10f;
            m_ButtonMoveObject.transform.localPosition = m_CurrentMovePosition;
        }
    }

    public float GetPlayerDistance()
    {
        if (TankCharacterController.Instance == null)
        {
            return 0f;
        }

        return Vector3.Distance(TankCharacterController.Instance.transform.position, transform.position);
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
        m_light.color = onColor;

        this.m_OnPressed?.Invoke();
        m_PressedSound?.Play();
    }
}
