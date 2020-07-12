using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Text m_ButtonsText;

    private float m_LastButtonCheck;

    void Update()
    {
        if (Time.time - m_LastButtonCheck > 0.2f)
        {
            m_LastButtonCheck = Time.time;

            var activators = Object.FindObjectsOfType<Activator>();
            var numButtons = 0;

            foreach (var activator in activators)
            {
                if (activator.Activated)
                {
                    numButtons++;
                }
            }

            if (activators.Length > 0)
            {
                if (!m_ButtonsText.gameObject.activeSelf)
                {
                    m_ButtonsText.gameObject.SetActive(true);
                }

                m_ButtonsText.text = numButtons + " / " + activators.Length;
            }
            else
            {
                if (m_ButtonsText.gameObject.activeSelf)
                {
                    m_ButtonsText.gameObject.SetActive(false);
                }
            }
        }
    }
}
