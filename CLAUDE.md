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
- **Joystick Pack** (`Assets/Joystick Pack`) — provides `DynamicJoystick`, the on-screen control wired through `InputManager` (see *Player & input* below).

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
- The concrete controller is `GameController` (`Scripts/Controller/`). It already wires the level lifecycle: `OnLevelLoaded` (registered on `LevelLoader.RegisterListenerFinishLoad`) is the **single place** that resets `GamePlayData` flags, sets `IsPlaying = true`, and calls `StartGameplay()`. The Snow Race mechanics themselves (`StartGameplay`/`StopGameplay`/`HandleWinGame`/`HandleLoseGame`) are still TODO stubs — gameplay reads components off `levelLoader.CurrentLevel` and signals the result by setting `GamePlayData.IsWin` then `PostEvent(EndGame)`.
- A win/lose is mistaken for a replay if `GamePlayData.IsReplay` isn't cleared — `HandleLoseGame` early-returns on it, and `OnLevelLoaded` clears it after `EndGame` has consumed it. Keep that ordering when extending.

### Player & input

The player stack lives under `Scripts/Player/` and is layered so the input source is swappable without touching movement:

- **`BaseInput`** (abstract) — an input source on the Player GameObject. Subclasses implement `HandleInput()` returning a world-space direction on the **X/Z plane** (Y unused). Implementations: `KeyboardInput` (WASD/arrows, Editor testing) and `InputController` (reads the joystick). Swap sources by changing which `BaseInput` component sits on the Player; movement code is unchanged.
- **`InputManager`** (singleton) — owns the `DynamicJoystick` and gates its visibility by game state: shown on `StartGame`, hidden on `EndGame`/`CancelGame`. `InputController` reads `InputManager.Instance.Direction` (zero while hidden) rather than touching the joystick directly.
- **`BaseMovement`** (abstract) — base for any mover (player/enemy/AI). Exposes state events (`OnRun`/`OnStop`/`OnLanding`/`OnLeaveGround`) and a `speedMultiplier` so boosters/slow-zones can scale speed without knowing the concrete type.
- **`BasePlayerMovement`** (abstract, `[RequireComponent(CharacterController)]`) — `CharacterController`-based movement with **manual gravity (no Rigidbody)**. Per `Update`: refresh grounded → gravity → jump (stub override point) → read `BaseInput` direction → turn heading toward it at `turnSpeed` → `CharacterController.Move`. Bails when `Time.timeScale == 0`.
- **`PlayerMovement`** (concrete) — fires `OnRun`/`OnStop` on movement-state changes and rotates the model to face horizontal velocity.

### Camera / object follow

`Scripts/Follow/` is a pre-placed-follower pattern (the follower moves its **own** transform; it does not parent or rotate):

- **`FollowController`** (singleton) — in `LateUpdate` picks the highest-`Priority` active `TargetFollow` and eases its own position toward that target's `TargetPoint`. A dead-zone of `radius` keeps it still until the target drifts past R; ties keep the current target (anti-thrash).
- **`TargetFollow`** — put on anything followable; registers/unregisters with `FollowController` in `OnEnable`/`OnDisable` and carries its own `offset` + `priority`.

### Snowball mechanic

`SnowBallController` (`Scripts/Player/`) is the core Snow Race growth mechanic. It does **not** move itself — it measures its own transform's per-frame position delta (something else, e.g. the player, moves it) and grows distance-based: `ΔR = distanceMoved * A(R)`, where `A` is sampled from a `conversionCurve` by normalized size (small ball grows fast, big ball slow) and clamped. The visual child (`ballModel`) scales to `radius`, is hidden at/below `minRadius`, capped at `maxRadius`, and rolls without slipping. Subscribe to `OnRadiusChanged(radius, maxRadius)` for UI.

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
