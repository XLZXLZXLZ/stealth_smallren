using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject m_EscapeMenu;
    public UnityEngine.UI.Text m_ResumeText;
    public UnityEngine.UI.Button m_ResumeButton;
    public UnityEngine.UI.Button m_LobbyButton;
    public UnityEngine.UI.Button m_QuitButton;

    private bool m_Open;
    private float m_LastAlive;

    private void Start()
    {
        m_ResumeButton.onClick.AddListener(this.OnResumeClicked);
        m_LobbyButton.onClick.AddListener(this.OnLobbyClicked);
        m_QuitButton.onClick.AddListener(this.OnQuitClicked);

        this.Close();
    }

    private void Update()
    {
        if (TankCharacterController.Instance.Alive)
        {
            m_LastAlive = Time.time;
            m_ResumeText.text = "Resume";
        }
        else
        {
            m_ResumeText.text = "Restart";

            if (Time.time - m_LastAlive > 2f)
            {
                this.Open();
            }
        }

        if (TankCharacterController.Instance == null || !TankCharacterController.Instance.Alive)
        {
            return;
        }

        if (Input.GetButtonDown("Pause"))
        {
            this.Toggle();
        }
    }

    public void Open()
    {
        m_Open = true;
        m_EscapeMenu.SetActive(true);

        if (TankCharacterController.Instance.Alive)
        {
            GameManager.Instance.Pause();
        }
    }

    public void Close()
    {
        m_Open = false;
        m_EscapeMenu.SetActive(false);

        GameManager.Instance.Unpause();
    }

    public void Toggle()
    {
        if (m_Open)
        {
            this.Close();
        }
        else
        {
            this.Open();
        }
    }

    private void OnResumeClicked()
    {
        this.Close();

        if (!TankCharacterController.Instance.Alive)
        {
            LevelManager.Instance.ReloadLevel();
        }
    }

    private void OnLobbyClicked()
    {
        LevelManager.Instance.LoadLobby();
    }

    private void OnQuitClicked()
    {
        Application.Quit();
    }
}
