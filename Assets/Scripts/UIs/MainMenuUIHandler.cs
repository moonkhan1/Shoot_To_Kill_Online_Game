using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenuUIHandler : MonoBehaviour
{
    [Header("Panels")] 
    [SerializeField] private GameObject _playerDetailPanel; 
    [SerializeField] private GameObject _sessionBrowsePanel; 
    [SerializeField] private GameObject _createSessionPanel; 
    [SerializeField] private GameObject _statusPanel; 
    
    [SerializeField] private TMP_InputField _playerNameinputField;
    [SerializeField] private TMP_InputField _sessionNameinputField;
    [SerializeField] private Button _findGameButton;
    [SerializeField] private Button _createGameButton;
    [SerializeField] private Button _startGameButton;

    
    private void Awake()
    {
        _findGameButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(_playerNameinputField.text))
            {
                OnFindGame();
            }
        });
        _startGameButton.onClick.AddListener(() =>
        {
            if (!string.IsNullOrEmpty(_sessionNameinputField.text))
            {
                OnStartNewSession();
            }
        });
        _createGameButton.onClick.AddListener(OnCreateNewGame);
    }

    private void HideAllPanels()
    {
        _playerDetailPanel.SetActive(false);
        _sessionBrowsePanel.SetActive(false);
        _createSessionPanel.SetActive(false);
        _statusPanel.SetActive(false);
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            _playerNameinputField.text = PlayerPrefs.GetString("PlayerName");
        }
    }

    public void OnJoiningServer()
    {
        HideAllPanels();

        _statusPanel.SetActive(true);
    }
    private void OnFindGame()
    {
        PlayerPrefs.SetString("PlayerName", _playerNameinputField.text);
        PlayerPrefs.Save();

        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        networkRunnerHandler.OnJoinLobby();
        HideAllPanels();
        _sessionBrowsePanel.SetActive(true);
        FindObjectOfType<SessionListHandler>(true).OnLookForSession();
        // FindObjectOfType<SessionListHandler>(true).OnLookForSession();
    }

    private void OnCreateNewGame()
    {
        HideAllPanels();
        
        _createSessionPanel.SetActive(true);
    }

    private void OnStartNewSession()
    {
        NetworkRunnerHandler networkRunnerHandler = FindObjectOfType<NetworkRunnerHandler>();
        
        networkRunnerHandler.CreateGame(_sessionNameinputField.text, "ReadyScene");
        
        HideAllPanels();
        _statusPanel.SetActive(true);
        Debug.Log($"{nameof(OnStartNewSession)}");
    }
}
