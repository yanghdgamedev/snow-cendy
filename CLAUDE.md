# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Unity **2022.3.62f2** mobile game. The codebase is a reusable game base (originally "ProjectGameBase") currently being built into **Snow Race**. Build target is Android (a `testBuild.apk` ships with the repo). There are no `.asmdef` files — all gameplay code compiles into the default `Assembly-CSharp`.

> The repo previously contained a sample "Arrow" puzzle game under `GameAssets/Scripts/Gameplay/`. It has been removed; only the reusable base (prefab-level loading + framework) remains. New Snow Race gameplay goes under `Assets/GameAssets/Scripts/`.

## Build, run, test

There is no CLI build/test wrapper — this is a standard Unity project driven from the Editor.

- **Open/run:** open the project in Unity 2022.3.62f2. The entry scene is `Assets/GameAssets/Scenes/LoadingGame.unity`, which async-loads into `GamePlay.unity` (see `BaseLoading`).
- **Tests:** the Unity Test Framework package is installed but no test assemblies/tests exist yet. Run tests via **Window → General → Test Runner** in the Editor.
- IDE project files (`Assembly-CSharp.csproj`, `*.sln`) are Unity-generated — do not hand-edit; they regenerate.

## Dependencies of note

- **Odin Inspector / Serializer** (`Assets/Plugins/Sirenix`) — classes that need dictionary/polymorphic serialization in the Inspector extend `SerializedMonoBehaviour` (e.g. `AudioManager`, `BaseLevel`). Use `[ShowIf]`, `[Header]` etc. as the codebase already does.
- **UniTask**, Unity Ads 4.4.2, Unity IAP (Purchasing) 4.11.0, TextMeshPro.

## Architecture

The code splits into two layers, by folder and namespace:

- **`Assets/GameDevelopmentKit/`** (namespaces `HDG.*` and `GameDevelopmentKit.Scripts`) — the **reusable engine/framework** shared across games. Treat it as a stable base; prefer extending it over modifying it.
- **`Assets/GameAssets/`** — the **game-specific** code, scenes, and resources for this title. Most feature work happens here.

### Event-driven game flow

Everything is decoupled through a global event bus, `HDG.EventDispatcher.EventDispatcher` (a `DontDestroyOnLoad` singleton, auto-created on first access). Use the `MonoBehaviour` extension methods rather than the singleton directly:

```csharp
this.RegisterListener(EventID.StartGame, OnStartGame);   // in OnEnable
this.PostEvent(EventID.EndGame);                          // fire
this.RemoveListener(EventID.StartGame, OnStartGame);      // in OnDisable
```

`EventID` is a single enum shared by the whole project — add new events there. The canonical lifecycle events are `StartGame`, `EndGame`, `ReplayGame`, `CancelGame`, `OnDestroyLevel`.

### Game controller + level lifecycle

`BaseGameController` (abstract) listens for the lifecycle events and drives the flow; concrete games subclass it and implement `HandleWinGame()` / `HandleLoseGame()`. It requires a sibling `LevelLoader`.

- `LevelLoader.LoadLevel(int)` does `Resources.Load<BaseLevel>(path + level)` then `Instantiate` (so a level is a **prefab** under `Resources/Levels/` whose root has a `BaseLevel`/`LevelManager` component, e.g. `Level_1.prefab`), and exposes `RegisterListenerFinishLoad` / `RegisterListenerBeginDestroy` callbacks. A concrete game controller hooks these to set up its gameplay once the level prefab is instantiated.
- **`GamePlayData`** is a global static holding the current match state (`IsPlaying`, `IsWin`, `IsEndGame`, `Level`, `IsReplay`, …). It is mutated directly across the codebase — be aware it is process-global, not per-instance. Call `GamePlayData.Reset()` to clear.
- The concrete controller is `GameController` (base flow + shows canvases); its `HandleWinGame()`/`HandleLoseGame()` are currently empty stubs. **This is where Snow Race gameplay gets built** — subclass/extend it and wire it to the loaded level prefab via the `LevelLoader` callbacks.

### UI canvases

UI screens extend `BaseBox` (`GameDevelopmentKit.Scripts`), a `Canvas`-backed view with auto-incrementing `sortingOrder` (stacking), `Show()`/`Close()`, and an `OnClose` callback. Concrete canvases (`HomeCanvas`, `GameCanvas`, `LoadingCanvas`) are **self-instantiating singletons** loaded from `Resources/UI/<Name>`:

```csharp
HomeCanvas.GetInstance().Show();   // Instantiate(Resources.Load<HomeCanvas>("UI/HomeCanvas"))
```

Canvas buttons typically just `PostEvent(...)` to drive flow (e.g. `GameCanvas.ReplayGame()` posts `ReplayGame`).

### Gameplay flow (current base)

The full loop today: `HomeCanvas.PlayGame()` posts `StartGame` → `BaseGameController.OnStartGame` calls `LevelLoader.LoadLevel(GameData.Level)` → the level prefab is instantiated → `GameController` shows `GameCanvas`. Win/lose is signalled by posting `EndGame` (with `GamePlayData.IsWin` set), which routes to `HandleWinGame`/`HandleLoseGame`. `ReplayGame`/`CancelGame` reload/tear down the level.

There is **no gameplay logic yet** beyond loading the prefab — Snow Race mechanics, win/lose conditions, and views are to be added.

### Persistence & helpers

- **`SystemData`** (extended by the empty `GameData`) is the single `PlayerPrefs` wrapper for all persisted state — `Level`, `Coin`/resources, `SoundOn`/`MusicOn`/`VibrationOn`, ad/open counters. Add new persisted values here; resources support change listeners via `RegisterResourceListener`.
- **`AudioManager`** (singleton) — `PlayMusic(MusicKey)` / `PlaySound(SoundKey)`; sounds/music are configured via Odin dictionaries and routed through an `AudioMixer`. Respects `GameData.SoundOn`/`MusicOn`. Add new clips by extending the `SoundKey`/`MusicKey` enums.
- **`Logger`** (`WDDebug.cs`) — use `Logger.Log/LogError/LogWarning` instead of `Debug.Log`; calls are compiled out when `ENV_PROD` is defined.
- **Object pooling** — `PoolManager` / `PoolObject` / `SOPrefab` under `Helper/ObjectPooling/`.

## Conventions

- Levels are **prefabs** under `Resources/Levels/`, named `Level_<N>.prefab`, with a `BaseLevel`/`LevelManager` component on the root (see `Level_1.prefab`). `LevelLoader` loads them by index.
- Anything loaded at runtime must sit under a `Resources/` folder (UI under `Resources/UI/`, levels under `Resources/Levels/`).
- Subscribe to events/level-loader callbacks in `OnEnable` and unsubscribe in `OnDisable` (the base controllers model this — follow it to avoid leaks with the global dispatcher).
