using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleLevelManager : MonoBehaviour
{
    public GameObject m_ElevatorPrefab;
    public GameObject m_PlayerPrefab;
    public GameObject m_LevelManagerPrefab;
    public GameObject m_GameManagerPrefab;
    public GameObject m_SoundManagerPrefab;

    [HideInInspector]
    public string m_LevelName;

    private bool m_Finished;

    private void Start()
    {
        var positionPlayer = false;

        var playerComponent = Object.FindObjectOfType<TankCharacterController>();
        var elevatorComponent = Object.FindObjectOfType<Elevator>();
        var elevatorTarget = Object.FindObjectOfType<LevelElevatorTarget>();

        if (playerComponent == null)
        {
            playerComponent = GameObject.Instantiate(m_PlayerPrefab).GetComponent<TankCharacterController>();
            elevatorComponent = GameObject.Instantiate(m_ElevatorPrefab).GetComponent<Elevator>();

            if (elevatorTarget != null)
            {
                elevatorComponent.SetNewDestination(elevatorTarget.transform.position);
                elevatorComponent.SetFinishedPreviously();
            }

            positionPlayer = true;
        }

        if (positionPlayer)
        {
            playerComponent.transform.position = elevatorComponent.transform.position + new Vector3(0f, 0.5f, 0f);
        }

        if (Object.FindObjectOfType<LevelManager>() == null)
        {
            GameObject.Instantiate(m_LevelManagerPrefab);
        }

        if (Object.FindObjectOfType<GameManager>() == null)
        {
            GameObject.Instantiate(m_GameManagerPrefab);
        }

        if (Object.FindObjectOfType<SoundManager>() == null)
        {
            GameObject.Instantiate(m_SoundManagerPrefab);
        }
    }

    public void FinishLevel()
    {
        if (m_Finished)
        {
            return;
        }

        m_Finished = true;

        var manager = Object.FindObjectOfType<LevelManager>();
        manager.LoadNextLevel();
    }
}
