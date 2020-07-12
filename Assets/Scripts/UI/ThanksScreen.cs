using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanksScreen : MonoBehaviour
{
    public static ThanksScreen Instance;

    public GameObject m_ThanksScreenObject;
    public Image m_ThanksImage;
    public Text m_ThanksText;

    private float m_ThankTime;
    private bool m_GoneToLobby;
    private float m_Alpha;
    private bool m_Thanked;

    private void Start()
    {
        Instance = this;
    }

    private void Update()
    {
        if (!m_Thanked && m_ThanksScreenObject.activeSelf)
        {
            if (m_Alpha > 0f)
            {
                m_Alpha -= Time.deltaTime;
            }
            else
            {
                m_ThanksScreenObject.SetActive(false);
                m_GoneToLobby = false;
            }
        }

        if (m_Thanked && !m_ThanksScreenObject.activeSelf)
        {
            m_ThanksScreenObject.SetActive(true);
            m_ThankTime = Time.time;
            m_GoneToLobby = false;
        }

        if (!m_Thanked)
        {
            var color = m_ThanksImage.color;
            color.a = m_Alpha;
            m_ThanksImage.color = color;

            var textColor = m_ThanksText.color;
            textColor.a = m_Alpha;
            m_ThanksText.color = textColor;

            return;
        }
        else
        {
            var deathDiff = Time.time - m_ThankTime;

            var color = m_ThanksImage.color;
            color.a = Mathf.Clamp(deathDiff - 2f / 1f, 0f, 1f);
            m_ThanksImage.color = color;

            var textColor = m_ThanksText.color;
            textColor.a = Mathf.Clamp(deathDiff - 4f / 1f, 0f, 1f);
            m_ThanksText.color = textColor;

            m_Alpha = Mathf.Max(m_Alpha, color.a);

            if (!m_GoneToLobby && deathDiff > 15f)
            {
                m_GoneToLobby = true;
                LevelManager.Instance.LoadLobby();
                m_Thanked = false;
            }
        }
    }

    public void Thank()
    {
        m_Thanked = true;
    }
}
