using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class InGameMessagesHandler : MonoBehaviour
{
    public TextMeshProUGUI[] InGameMessages;

    private Queue messageQueue = new();

    // [Inject] private NetworkInGameMessagesManager _networkInGameMessagesManager;

    public void OnGameMessageReceived(string message)
    {
        messageQueue.Enqueue(message);

        if (messageQueue.Count > 3) messageQueue.Dequeue();
        int queueIndex = 0;
        foreach (string messageInQueue in messageQueue)
        {
            InGameMessages[queueIndex].text = messageInQueue;
            queueIndex++;
        }
    }

}
