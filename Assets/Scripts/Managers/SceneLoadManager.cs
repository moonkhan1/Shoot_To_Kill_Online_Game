using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoadManager : SingletonBase<SceneLoadManager>
{
    private void Awake()
    {
        MakeSingleton(this);
    }

    public async void LoadGame()
    {
        await SceneLoader.LoadSceneAsyncCustom(SceneLoader.Scene.Game, 2f);
    }
    public async void LoadMenu()
    {
        await SceneLoader.LoadSceneAsyncCustom(SceneLoader.Scene.MenuScene, 2f);
    }
}
