using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public bool m_StartOpen;
    public float m_Speed = 1f;
    public Vector3 m_MoveDirection = Vector3.one;

    private bool m_Open;
    private Vector3 m_OriginalPosition;

    public bool IsFullyClosed
        => Vector3.Distance(transform.localPosition, m_OriginalPosition) <= 0.01f;

    public bool IsOpen
        => m_Open;

    void Start()
    {
        m_Open = m_StartOpen;
        m_OriginalPosition = transform.localPosition;
    }

    void Update()
    {
        var target = m_Open ? m_OriginalPosition + m_MoveDirection : m_OriginalPosition;

        transform.localPosition += (target - transform.localPosition) * Time.deltaTime * m_Speed;
    }

    public void SetOpen(bool open)
    {
        m_Open = open;
    }

    public void Open()
    {
        m_Open = true;
    }

    public void Close()
    {
        m_Open = false;
    }
}
