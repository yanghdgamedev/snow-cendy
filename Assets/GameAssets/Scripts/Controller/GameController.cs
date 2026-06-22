using System;
using System.Collections;
using System.Collections.Generic;
using HDG.BaseGameController;
using HDG.EventDispatcher;
using UnityEngine;

public class GameController : BaseGameController
{
    private void Awake()
    {
        HomeCanvas.GetInstance().Show();
    }

    protected override void OnStartGame(object obj)
    {
        base.OnStartGame(obj);
        GameCanvas.GetInstance().Show();
    }

    protected override void LoadLevel(int levelPlay)
    {
        base.LoadLevel(levelPlay);
    }

    protected override void OnReplayGame(object obj)
    {
        base.OnReplayGame(obj);
    }

    protected override void OnCancelGame(object obj)
    {
        base.OnCancelGame(obj);
    }

    protected override void HandleWinGame()
    {
        
    }

    protected override void HandleLoseGame()
    {
        if (!GamePlayData.IsReplay)
        {
            
        }
    }
}
