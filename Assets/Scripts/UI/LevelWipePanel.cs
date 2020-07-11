using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelWipePanel : MonoBehaviour
{
    public static LevelWipePanel Instance;

    public float m_WipeSpeed = 1f;

    private Image m_Image;
    private float m_WipeAmount = 0f;

    public bool IsFullyWiped
        => m_WipeAmount == 1f;

    public bool WipedDown { get; set; }

    private void Start()
    {
        if (LevelManager.Instance.LevelIndex >= 0)
        {
            this.WipedDown = true;
            m_WipeAmount = 1f;
        }

        m_Image = gameObject.GetComponent<Image>();
        m_Image.fillAmount = m_WipeAmount;

        Instance = this;
    }

    private void Update()
    {
        var target = this.WipedDown ? 1f : 0f;

        m_WipeAmount += (target - m_WipeAmount) * Time.deltaTime * m_WipeSpeed;

        if (Mathf.Abs(target - m_WipeAmount) <= 0.01f)
        {
            m_WipeAmount = target;
        }

        m_Image.fillAmount = m_WipeAmount;
    }
}
