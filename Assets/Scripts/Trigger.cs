using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
    public bool m_PlayerOnly = false;
    public int m_TriggerLimit = 0;
    public UnityEvent m_OnTrigger;

    private int m_TriggerCount;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void DoTrigger()
    {
        m_TriggerCount++;
        m_OnTrigger.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (m_TriggerLimit > 0 && m_TriggerLimit <= m_TriggerCount)
        {
            return;
        }

        var rb = other.attachedRigidbody;

        if (rb == null)
        {
            return;
        }

        if (m_PlayerOnly)
        {
            var controller = rb.gameObject.GetComponent<TankCharacterController>();

            if (controller == null)
            {
                return;
            }
        }

        this.DoTrigger();
    }
}
