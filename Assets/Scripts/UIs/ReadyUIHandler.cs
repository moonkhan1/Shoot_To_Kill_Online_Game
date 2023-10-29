using Fusion;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyUIHandler : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _readyText;
    [SerializeField] private TextMeshProUGUI _countText;
    
    private bool IsReady;
    TickTimer countDownTickTimer = TickTimer.None;

    [Networked(OnChanged = nameof(OnCountdownChanged))]
    private int countDown { get; set; }

    private void Start()
    {
        _countText.text = "";
    }

    private void Update()
    {
        if(countDownTickTimer.Expired(Runner))
        {
            StartGame();
            countDownTickTimer = TickTimer.None;
        }
        else if(countDownTickTimer.IsRunning)
        {
            countDown = (int)countDownTickTimer.RemainingTime(Runner);
        }
    }

    public void OnPlayerModelChanged()
    {
        if(IsReady) return;

        NetworkPlayer.Local.GetComponent<CharacterModelChangeHandler>().ModelChangeCycle();
    }
    private void StartGame()
    {
        //Lock the session when host decide to start game to not allow other clients joint
        Runner.SessionInfo.IsOpen = false;

        GameObject[] playersDontDestroyOnLoad = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject playerDontDestroyOnLoad  in playersDontDestroyOnLoad)
        {
            DontDestroyOnLoad(playerDontDestroyOnLoad);

            // Kick the client if is not done with model selection
            if(!playerDontDestroyOnLoad.GetComponent<CharacterModelChangeHandler>().isDoneWithModelSelection)
            {
                Runner.Disconnect(playerDontDestroyOnLoad.GetComponent<NetworkObject>().InputAuthority);
            }
        }

        Runner.SetActiveScene("Game1");
    }
    
    public void OnReady()
    {
        if (IsReady)
        {
            IsReady = false;
        }
        else
        {
            IsReady = true;
        }

        if (IsReady)
        {
            _readyText.text = "Not Ready";
        }
        else
        {
            _readyText.text = "Ready";
        }

        if (Runner.IsServer)
        {
            if(IsReady)
            {
                countDownTickTimer = TickTimer.CreateFromSeconds(Runner, 10);
            }
            else
            {
                countDownTickTimer = TickTimer.None;
                countDown = 0;
            }
        }

        NetworkPlayer.Local.GetComponent<CharacterModelChangeHandler>().OnReady(IsReady);
    }
    private static void OnCountdownChanged(Changed<ReadyUIHandler> changed)
    {
        changed.Behaviour.OnCountdownChanged();
    }
    private void OnCountdownChanged()
    {
        Debug.Log("On COUNTDOWN");
        if (countDown == 0)
        {
            _countText.text = $"";
        }
        else
        {
            _countText.text = $"Starts in {countDown}";
        }
    }


}
