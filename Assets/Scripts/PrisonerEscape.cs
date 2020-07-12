using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PrisonerEscape : MonoBehaviour
{
    public int m_EscapeCount;
    public UnityEvent m_OnAllEscaped;

    private float m_EscapedCount;

    public void AddEscape()
    {
        m_EscapedCount++;

        if (m_EscapedCount >= m_EscapeCount)
        {
            m_OnAllEscaped?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.4f);
    }
}
