using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public enum Scene
    {
        MenuScene,
        Game
    }

    private static Scene _targetScene;

    public static async UniTask LoadSceneAsyncCustom(Scene targetScene, double loadingSceneDuration)
    {
        _targetScene = targetScene;
        // SceneManager.LoadScene(Scene.LoadingScene.ToString()); // Load Loading Scene before any scene

        await UniTask.Delay(TimeSpan.FromSeconds(loadingSceneDuration)); // Delay for given seconds

        SceneManager.LoadSceneAsync(_targetScene.ToString());
    }
}
