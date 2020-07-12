using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public static Elevator ExitElevator;
    public static Elevator PrevExitElevator;

    public float m_Speed;
    public float m_Acceleration;
    public bool m_StartOpen;
    public int m_ActivationCount;
    public Door m_RightDoor1;
    public Door m_LeftDoor1;
    public Door m_RightDoor2;
    public Door m_LeftDoor2;
    public AudioSource m_DoorAudio;
    public AudioSource m_StartSound;
    public AudioSource m_LoopSound;
    public AudioSource m_StopSound;

    private float m_CurrentSpeed;
    private Vector3 m_TargetPosition;
    private bool m_GoingUp;
    private bool m_Finished;
    private int m_NumActivations;
    private bool m_Moving;
    private float m_StartMovingTime;
    private float m_LastSpeed;
    private bool m_IsSlowingDown;

    private void Awake()
    {
        m_TargetPosition = transform.position;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        if (m_StartOpen)
        {
            m_LeftDoor1.Open();
            m_RightDoor1.Open();
        }
    }

    private void Update()
    {
        var targetSpeed = 0f;

        if (!m_Finished && m_NumActivations >= m_ActivationCount && !m_LeftDoor1.IsOpen)
        {
            m_LeftDoor1.Open();
            m_RightDoor1.Open();

            m_DoorAudio.Play();
        }

        if (!m_Finished || !this.AreDoorsClosed())
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

        if (m_CurrentSpeed < m_LastSpeed)
        {
            m_IsSlowingDown = true;
        }

        var increase = new Vector3(0f, m_CurrentSpeed * Time.deltaTime, 0f);
        var moving = increase.magnitude > 0.001f;

        if (moving != m_Moving && !m_IsSlowingDown)
        {
            if (m_LoopSound.isPlaying)
            {
                m_LoopSound.Stop();
            }

            if (moving)
            {
                m_StartSound.Play();
                m_StartMovingTime = Time.time;
            }

            m_Moving = moving;
        }

        if (!moving)
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

                m_DoorAudio.Play();
            }

            return;
        }
           
        if (m_IsSlowingDown)
        {
            if (m_LoopSound.isPlaying)
            {
                m_LoopSound.Stop();
                m_StopSound.Play();
            }
        }
        else if (!m_LoopSound.isPlaying && Time.time - m_StartMovingTime > m_StartSound.clip.length)
        {
            m_LoopSound.Play();
            m_StartSound.Stop();
        }

        m_LastSpeed = m_CurrentSpeed;
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

    public void AddActivation()
    {
        m_NumActivations++;
    }

    public void FinishLevel()
    {
        if (m_Finished)
        {
            return;
        }

        if (ExitElevator != this)
        {
            PrevExitElevator = ExitElevator;
        }

        ExitElevator = this;

        m_Finished = true;

        var manager = Object.FindObjectOfType<LevelManager>();
        manager.LoadNextLevel();
    }

    public void SetFinishedPreviously()
    {
        m_Finished = true;
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

        m_IsSlowingDown = false;

        m_DoorAudio.Play();
    }

    public void SetNewDestination(Vector3 destination)
    {
        var player = TankCharacterController.Instance;
        var pPos = Vector3.zero;

        if (player != null)
        {
            pPos = TankCharacterController.Instance.transform.position - transform.position;
        }

        transform.position = destination - new Vector3(0f, 10f, 0f);
        m_GoingUp = false;

        if (TankCharacterController.Instance != null)
        {
            TankCharacterController.Instance.transform.position = transform.position + pPos;
        }

        m_TargetPosition = destination;
    }

    private void OnLevelWasLoaded(int level)
    {
        if (PrevExitElevator == this)
        {
            Destroy(gameObject);
        }
    }
}
