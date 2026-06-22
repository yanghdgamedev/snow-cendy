using HDG.BaseGameController;
using HDG.EventDispatcher;
using UnityEngine;

/// <summary>
/// Snow Race game controller.
///
/// Lifecycle (driven by BaseGameController + EventDispatcher):
///   Awake                 → show Home.
///   StartGame             → base loads the level prefab → OnLevelLoaded → show GameCanvas.
///   (gameplay sets IsWin)  → PostEvent(EndGame) → HandleWinGame / HandleLoseGame.
///   ReplayGame            → EndGame + StartGame (reloads a fresh level).
///   CancelGame            → DestroyLevel → OnLevelDestroyed.
///
/// All Snow Race mechanics hang off OnLevelLoaded (the spawned level prefab is
/// available via levelLoader.CurrentLevel).
/// </summary>
public class GameController : BaseGameController
{
    private BaseLevel _level;

    private void Awake()
    {
        HomeCanvas.GetInstance().Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Hook the level lifecycle so gameplay starts/stops with the prefab.
        levelLoader.RegisterListenerFinishLoad(OnLevelLoaded);
        levelLoader.RegisterListenerBeginDestroy(OnLevelDestroyed);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (levelLoader != null)
        {
            levelLoader.RemoveListenerFinishLoad(OnLevelLoaded);
            levelLoader.RemoveListenerBeginDestroy(OnLevelDestroyed);
        }
    }

    protected override void OnStartGame(object obj)
    {
        base.OnStartGame(obj); // loads the level prefab → fires OnLevelLoaded synchronously
        GameCanvas.GetInstance().Show();
    }

    /// <summary>
    /// Called right after the level prefab is instantiated. This is the single
    /// place that (re)initializes match state and boots the gameplay.
    /// </summary>
    private void OnLevelLoaded(object levelIndex)
    {
        _level = levelLoader.CurrentLevel;

        // FIX #2: reset stale flags so a new match never inherits the previous result.
        // FIX #3: clear IsReplay here (after EndGame already consumed it during a replay)
        //         so a later natural lose isn't mistaken for a replay.
        GamePlayData.IsWin = false;
        GamePlayData.IsLose = false;
        GamePlayData.IsCancel = false;
        GamePlayData.IsEndGame = false;
        GamePlayData.IsReplay = false;

        // FIX #1: nobody set IsPlaying = true → EndGame was ignored and win/lose never fired.
        GamePlayData.IsPlaying = true;

        StartGameplay();
    }

    private void OnLevelDestroyed(object levelIndex)
    {
        StopGameplay();
        _level = null;
        GamePlayData.IsPlaying = false;
    }

    // --- Snow Race gameplay hooks (to implement) -------------------------------

    /// <summary>Boot Snow Race for the loaded level: spawn racer, build track, start timer…</summary>
    private void StartGameplay()
    {
        // TODO: read components off `_level` and start the race.
        // When the racer reaches the finish line:  GamePlayData.IsWin = true; this.PostEvent(EventID.EndGame);
        // When the racer fails (fall/time out):     GamePlayData.IsWin = false; this.PostEvent(EventID.EndGame);
    }

    /// <summary>Tear down anything spawned for the current race.</summary>
    private void StopGameplay()
    {
        // TODO: unsubscribe gameplay events, stop coroutines, release pooled objects.
    }

    protected override void HandleWinGame()
    {
        // TODO: show win UI, advance GameData.Level, save progress.
    }

    protected override void HandleLoseGame()
    {
        if (GamePlayData.IsReplay)
        {
            return; // a replay-triggered EndGame is not a real loss
        }
        // TODO: show lose UI / retry prompt.
    }
}
