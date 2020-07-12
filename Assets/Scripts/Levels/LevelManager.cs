using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public Level m_LobbyLevel = new Level()
    {
        m_Name = "Lobby",
        m_SceneName = "MainMenu"
    };
    public GameObject m_CharacterPrefab;
    public Level[] m_Levels;

    private int m_LevelIndex = 0;
    private LevelSpawnPoint m_SpawnPoint;
    private GameObject m_CharacterObject;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        this.SetupLevel();
    }

    public int LevelIndex
        => m_LevelIndex;

    private void Update()
    {

    }

    public void SetLevel(int level)
    {
        m_LevelIndex = level;
    }

    public void LoadCurrentLevel()
    {
        this.StartCoroutine(this.LoadNextLevelRoutine());
    }

    public void LoadNextLevel()
    {
        m_LevelIndex++;

        this.StartCoroutine(this.LoadNextLevelRoutine());
    }

    public void ReloadLevel()
    {
        if (Elevator.LastSpawnedElevator != null && Elevator.LastSpawnedElevator != Elevator.LastEnterElevator)
        {
            Destroy(Elevator.LastSpawnedElevator.gameObject);
        }

        var level = m_Levels[m_LevelIndex];
        SceneManager.LoadScene(level.m_SceneName);

        if (TankCharacterController.Instance != null)
        {
            if (Elevator.ExitElevator != null)
            {
                TankCharacterController.Instance.transform.position = Elevator.ExitElevator.transform.position;
            }
            else if(Elevator.LastEnterElevator != null)
            {
                TankCharacterController.Instance.transform.position = Elevator.LastEnterElevator.transform.position;
            }
            
            TankCharacterController.Instance.Revive();
        }

        SoundManager.Instance.SetMusicType(SoundManager.Instance.m_DefaultMusicType);
    }

    public void LoadLobby()
    {
        m_LevelIndex = 0;

        if (TankCharacterController.Instance != null)
        {
            Destroy(TankCharacterController.Instance.gameObject);
        }

        if (Elevator.ExitElevator != null)
        {
            Destroy(Elevator.ExitElevator.gameObject);
        }

        if (Elevator.PrevExitElevator != null)
        {
            Destroy(Elevator.PrevExitElevator.gameObject);
        }

        if (Elevator.LastSpawnedElevator != null)
        {
            Destroy(Elevator.LastSpawnedElevator.gameObject);
        }

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }

        if (SoundManager.Instance != null)
        {
            Destroy(SoundManager.Instance.gameObject);
        }

        SceneManager.LoadScene(m_LobbyLevel.m_SceneName);

        Destroy(gameObject);
    }

    private void SetupLevel()
    {
        m_SpawnPoint = Object.FindObjectOfType<LevelSpawnPoint>();

        var controllerComponent = Object.FindObjectOfType<TankCharacterController>();

        if (controllerComponent == null)
        {
            m_CharacterObject = GameObject.Instantiate(m_CharacterPrefab);
        }
        else
        {
            m_CharacterObject = controllerComponent.gameObject;
        }

        if (m_SpawnPoint != null && m_CharacterObject != null)
        {
            m_CharacterObject.transform.position = m_SpawnPoint.transform.position;
        }

        if (m_LevelIndex >= 0)
        {
            var singleLevelManager = Object.FindObjectOfType<SingleLevelManager>();

            if (singleLevelManager != null)
            {
                singleLevelManager.m_LevelName = m_Levels[m_LevelIndex].m_Name;
            }
        }

        SoundManager.Instance.SetMusicType(SoundManager.Instance.m_DefaultMusicType);
    }

    private IEnumerator LoadNextLevelRoutine()
    {
        /*var wipe = LevelWipePanel.Instance;
        wipe.WipedDown = true;

        while (!wipe.IsFullyWiped)
        {
            yield return null;
        }*/

        Elevator.ExitElevator.GoUp();

        while (!Elevator.ExitElevator.AreDoorsClosed())
        {
            yield return null;
        }

        yield return new WaitForSeconds(4.5f);

        var level = m_Levels[m_LevelIndex];
        var asyncLoad = SceneManager.LoadSceneAsync(level.m_SceneName);

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        this.SetupLevel();

        var elevatorTarget = Object.FindObjectOfType<LevelElevatorTarget>();
        Elevator.ExitElevator.SetNewDestination(elevatorTarget.transform.position);

        yield return new WaitForSeconds(0.1f);
        if (LevelWipePanel.Instance != null)
        {
            LevelWipePanel.Instance.WipedDown = false;
        }
    }
}

[System.Serializable]
public class Level
{
    public string m_Name;
    public string m_SceneName;
}