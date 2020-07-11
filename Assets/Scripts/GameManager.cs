using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static string SelectedLevelKey = "selected_level";
    public static string LevelUnlockedKey = "level_unlocked_";

    public static GameManager Instance;
    public int m_SelectedLevel = 0;

    private void Awake()
    {
        Instance = this;

        m_SelectedLevel = PlayerPrefs.GetInt(SelectedLevelKey, 0);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void CycleLevel()
    {
        m_SelectedLevel++;

        if (!this.LevelUnlocked(m_SelectedLevel))
        {
            m_SelectedLevel = 0;
        }
    }

    public void SelectLevel(int level)
    {
        m_SelectedLevel = level;
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
