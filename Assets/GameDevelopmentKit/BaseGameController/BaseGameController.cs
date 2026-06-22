using System.Collections.Generic;
using HDG.EventDispatcher;
using UnityEngine;

namespace HDG.BaseGameController
{
    [RequireComponent(typeof(LevelLoader))]
    public abstract class BaseGameController : MonoBehaviour
    {
        [SerializeField] protected LevelLoader levelLoader;
        
        protected virtual void OnEnable()
        {
            this.RegisterListener(EventID.StartGame, OnStartGame);
            this.RegisterListener(EventID.EndGame, OnEndGame);
            this.RegisterListener(EventID.CancelGame, OnCancelGame);
            this.RegisterListener(EventID.ReplayGame, OnReplayGame);
        }

        protected virtual void OnDisable()
        {
            this.RemoveListener(EventID.StartGame, OnStartGame);
            this.RemoveListener(EventID.EndGame, OnEndGame);
            this.RemoveListener(EventID.CancelGame, OnCancelGame);
            this.RemoveListener(EventID.ReplayGame, OnReplayGame);
        }
        protected virtual void OnEndGame(object obj)
        {
            if (!GamePlayData.IsPlaying)
            {
                return;
            }
            GamePlayData.IsPlaying = false;
            GamePlayData.IsEndGame = true;
            GamePlayData.EndTime = Time.unscaledTime;
            if (GamePlayData.IsWin)
            {
                HandleWinGame();
            }
            else
            {
                HandleLoseGame();
            }
            Logger.Log("<color=pink>End Game</color>");
        }

        protected virtual void LoadLevel(int levelPlay)
        {
            GamePlayData.StartTime = Time.unscaledTime;
            levelLoader.LoadLevel(levelPlay);
        }

        protected virtual void OnStartGame(object obj)
        {
            if (levelLoader.IsLevelLoaded && !GamePlayData.IsEndGame)
            {
                return;
            }
            LoadLevel(GameData.Level);
        }

        protected virtual void OnReplayGame(object obj)
        {
            GamePlayData.IsReplay = true;
            this.PostEvent(EventID.EndGame);
            this.PostEvent(EventID.StartGame);
        }

        protected virtual void OnCancelGame(object obj)
        {
            levelLoader.DestroyLevel();
        }
        protected abstract void HandleWinGame();
        protected abstract void HandleLoseGame();
    }
}

public class GamePlayData
{
    public static float StartTime;
    public static float EndTime;
    public static int Level = 1;
    public static bool IsWin = false;
    public static bool IsPlaying = false;
    public static bool IsCancel = false;
    public static bool IsLose = false;
    public static bool IsReplay = false;
    public static bool IsEndGame = false;
    public static LevelManager LevelManager;

    public static void Reset()
    {
        StartTime = 0;
        EndTime = 0;
        Level = -1;
        IsWin = false;
        IsLose = false;
        IsPlaying = false;
        IsCancel = false;
        IsReplay = false;
        IsEndGame = false;
        LevelManager = null;
    }
}