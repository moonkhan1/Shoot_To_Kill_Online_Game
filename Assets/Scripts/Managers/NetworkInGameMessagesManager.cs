using System;
using Fusion;
using UnityEngine;

public class NetworkInGameMessagesManager : NetworkBehaviour
{
    public event Action<string> OnInGameMessage;
    InGameMessagesHandler _inGameMessagesHandler;
    

    public void SendInGameRpcMessages(string userName, string message)
    {
        string formattedMessage = $"<b>{userName}</b>{message}";
        
        RPC_InGameMessage(formattedMessage);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_InGameMessage(string message, RpcInfo info = default)
    {
        if (_inGameMessagesHandler == null)
        {
            _inGameMessagesHandler =
                NetworkPlayer.Local.localCameraHandler.GetComponentInChildren<InGameMessagesHandler>();
        }
        if (_inGameMessagesHandler != null)
        {
            _inGameMessagesHandler.OnGameMessageReceived(message);
        }
    }
}
