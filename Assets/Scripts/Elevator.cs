using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public static Elevator ExitElevator;

    public float m_Speed;
    public float m_Acceleration;
    public Door m_RightDoor1;
    public Door m_LeftDoor1;
    public Door m_RightDoor2;
    public Door m_LeftDoor2;

    private float m_CurrentSpeed;
    private Vector3 m_TargetPosition;
    private bool m_GoingUp;
    private bool m_Finished;

    private void Awake()
    {
        
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        m_TargetPosition = transform.position;
    }

    private void Update()
    {
        var targetSpeed = 0f;

        if (!this.AreDoorsClosed())
        {
            return;
        }

        if (m_GoingUp)
        {
            targetSpeed = m_Speed;
            m_CurrentSpeed += (targetSpeed - m_CurrentSpeed) * Time.deltaTime * m_Acceleration;
        }
        else
        {
            var diffY = m_TargetPosition.y - transform.position.y;

            m_CurrentSpeed = Mathf.Clamp(diffY * 2f, -m_Speed, m_Speed);
        }

        var increase = new Vector3(0f, m_CurrentSpeed * Time.deltaTime, 0f);

        if (increase.magnitude <= 0.0001f)
        {
            if (!m_GoingUp)
            {
                if (!m_RightDoor2.IsOpen)
                {
                    m_RightDoor2.Open();
                }

                if (!m_LeftDoor2.IsOpen)
                {
                    m_LeftDoor2.Open();
                }
            }

            return;
        }

        var distance = Vector3.Distance(m_TargetPosition, transform.position);

        transform.position += increase;
        TankCharacterController.Instance.transform.position += increase;

        if (!m_GoingUp)
        {
            if (distance < Vector3.Distance(m_TargetPosition, transform.position))
            {
                transform.position = m_TargetPosition;
                m_CurrentSpeed = 0f;
            }
        }
    }

    public void FinishLevel()
    {
        if (m_Finished)
        {
            return;
        }

        ExitElevator = this;

        m_Finished = true;

        var manager = Object.FindObjectOfType<LevelManager>();
        manager.LoadNextLevel();
    }

    public bool AreDoorsClosed()
    {
        return m_LeftDoor1.IsFullyClosed && m_RightDoor1.IsFullyClosed && m_LeftDoor2.IsFullyClosed && m_RightDoor2.IsFullyClosed;
    }
    public void GoUp()
    {
        m_GoingUp = true;

        m_LeftDoor1.Close();
        m_LeftDoor2.Close();
        m_RightDoor1.Close();
        m_RightDoor2.Close();
    }

    public void SetNewDestination(Vector3 destination)
    {
        var pPos = TankCharacterController.Instance.transform.position - transform.position;

        transform.position = destination - new Vector3(0f, 10f, 0f);
        m_GoingUp = false;

        TankCharacterController.Instance.transform.position = transform.position + pPos;

        m_TargetPosition = destination;
    }
}
