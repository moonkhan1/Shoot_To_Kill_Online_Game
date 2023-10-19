using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class DestroyGameObject : MonoBehaviour
{
    private float lifeSpan = 1.6f;

    private void Start()
    {
        DestroyAsync();
    }
    private async void DestroyAsync()
    {
        await UniTask.WaitForSeconds(lifeSpan);
        Destroy(gameObject);
    }
}
