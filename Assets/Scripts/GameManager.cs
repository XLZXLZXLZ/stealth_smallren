using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string SelectedLevelKey = "selected_level";
    public static string LevelUnlockedKey = "level_unlocked_";

    public static GameManager Instance;
    public int m_SelectedLevel = 0;

    private bool m_Paused;

    public bool Paused
        => m_Paused;

    private void Awake()
    {
        Instance = this;

        m_SelectedLevel = PlayerPrefs.GetInt(SelectedLevelKey, 0);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        m_Paused = true;
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
        m_Paused = false;
    }

    public void CycleLevel(int levels)
    {
        m_SelectedLevel = Mathf.Max(0, m_SelectedLevel + levels);

        if (!this.LevelUnlocked(m_SelectedLevel))
        {
            m_SelectedLevel--;
        }

        LevelManager.Instance.SetLevel(m_SelectedLevel);
    }

    public void SelectLevel(int level)
    {
        m_SelectedLevel = level;
        LevelManager.Instance.SetLevel(m_SelectedLevel);
    }

    public bool LevelUnlocked(int level)
    {
        if (level == 0)
        {
            return true;
        }

        return PlayerPrefs.GetInt(LevelUnlockedKey + level.ToString(), 0) == 1;
    }

    public void UnlockLevel(int level)
    {
        PlayerPrefs.SetInt(LevelUnlockedKey + level.ToString(), 1);
        m_SelectedLevel = Mathf.Max(level, PlayerPrefs.GetInt(SelectedLevelKey, 0));
    }
}
