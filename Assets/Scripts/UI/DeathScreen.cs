using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    public GameObject m_DeathScreenObject;
    public Image m_DeathImage;
    public Text m_DeathText;

    private float m_DeathTime;
    private bool m_Reloaded;

    void Update()
    {
        if (!TankCharacterController.Instance)
        {
            return;
        }

        if (TankCharacterController.Instance.Alive && m_DeathScreenObject.activeSelf)
        {
            m_DeathScreenObject.SetActive(false);
        }

        if(!TankCharacterController.Instance.Alive && !m_DeathScreenObject.activeSelf)
        {
            m_DeathScreenObject.SetActive(true);
            m_DeathTime = Time.time;
            m_Reloaded = false;
        }

        if (TankCharacterController.Instance.Alive)
        {
            return;
        }

        var color = m_DeathImage.color;
        color.a = Mathf.Clamp(m_DeathTime - 1 / 1f, 0f, 1f);
        m_DeathImage.color = color;

        var textColor = m_DeathText.color;
        textColor.a = Mathf.Clamp(m_DeathTime - 2.5f / 1f, 0f, 1f);
        m_DeathText.color = textColor;

        if (!m_Reloaded && Time.time - m_DeathTime > 6f)
        {
            m_Reloaded = true;
            LevelManager.Instance.ReloadLevel();
        }
    }
}
