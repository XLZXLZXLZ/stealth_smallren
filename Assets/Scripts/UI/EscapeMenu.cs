using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject m_EscapeMenu;
    public UnityEngine.UI.Button m_ResumeButton;
    public UnityEngine.UI.Button m_LobbyButton;
    public UnityEngine.UI.Button m_QuitButton;

    private bool m_Open;

    private void Start()
    {
        m_ResumeButton.onClick.AddListener(this.OnResumeClicked);
        m_LobbyButton.onClick.AddListener(this.OnLobbyClicked);
        m_QuitButton.onClick.AddListener(this.OnQuitClicked);

        this.Close();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            this.Toggle();
        }
    }

    public void Open()
    {
        m_Open = true;
        m_EscapeMenu.SetActive(true);

        GameManager.Instance.Pause();
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
