using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public GameObject m_CharacterPrefab;
    public Level[] m_Levels;

    private int m_LevelIndex = -1;
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

    public void LoadNextLevel()
    {
        m_LevelIndex++;

        this.StartCoroutine(this.LoadNextLevelRoutine());
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
            singleLevelManager.m_LevelName = m_Levels[m_LevelIndex].m_Name;
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

        yield return new WaitForSeconds(2.5f);

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
        LevelWipePanel.Instance.WipedDown = false;
    }
}

[System.Serializable]
public class Level
{
    public string m_Name;
    public string m_SceneName;
}