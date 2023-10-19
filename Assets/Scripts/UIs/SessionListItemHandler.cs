using System;
using System.Collections;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListItemHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sessionNameText; 
    [SerializeField] private TextMeshProUGUI _playerCountText; 
    [SerializeField] private Button _joinButton; 
    private SessionInfo _sessionInfo;

    public event Action<SessionInfo> OnJoinSession; 
    private void Awake()
    {
        _joinButton.onClick.AddListener(OnClickJoin);
    }

    public void SetInfo(SessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;

        _sessionNameText.text = sessionInfo.Name;
        _playerCountText.text = $"{sessionInfo.PlayerCount} / {sessionInfo.MaxPlayers}";

        bool isJoinButtonActive = !(sessionInfo.PlayerCount >= sessionInfo.MaxPlayers);
        _joinButton.gameObject.SetActive(isJoinButtonActive);
    }
    private void OnClickJoin()
    {
        OnJoinSession?.Invoke(_sessionInfo);
    }
}
