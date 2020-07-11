using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleLevelManager : MonoBehaviour
{
    [HideInInspector]
    public string m_LevelName;

    private bool m_Finished;

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
