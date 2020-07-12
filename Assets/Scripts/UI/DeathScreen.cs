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
    private float m_Alpha;

    void Update()
    {
        if (!TankCharacterController.Instance)
        {
            return;
        }

        if (TankCharacterController.Instance.Alive && m_DeathScreenObject.activeSelf)
        {
            if (m_Alpha > 0f)
            {
                m_Alpha -= Time.deltaTime;
            }
            else
            {
                m_DeathScreenObject.SetActive(false);
            }
        }

        if(!TankCharacterController.Instance.Alive && !m_DeathScreenObject.activeSelf)
        {
            m_DeathScreenObject.SetActive(true);
            m_DeathTime = Time.time;
            m_Reloaded = false;
        }

        if (TankCharacterController.Instance.Alive)
        {
            var color = m_DeathImage.color;
            color.a = m_Alpha;
            m_DeathImage.color = color;

            var textColor = m_DeathText.color;
            textColor.a = m_Alpha;
            m_DeathText.color = textColor;

            return;
        }
        else
        {
            var deathDiff = Time.time - m_DeathTime;

            var color = m_DeathImage.color;
            color.a = Mathf.Clamp(deathDiff - 3f / 1f, 0f, 1f);
            m_DeathImage.color = color;

            var textColor = m_DeathText.color;
            textColor.a = Mathf.Clamp(deathDiff - 4.5f / 1f, 0f, 1f);
            m_DeathText.color = textColor;

            m_Alpha = Mathf.Max(m_Alpha, color.a);

            if (!m_Reloaded && deathDiff > 9f)
            {
                m_Reloaded = true;
                LevelManager.Instance.ReloadLevel();
            }
        }
    }
}
