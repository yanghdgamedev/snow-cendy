# Arrow Gameplay System

Implementation của Arrow Maze gameplay (cách 2: LineRenderer + SpriteRenderer head + MeshCollider) bám HDG framework.

## Cài đặt

1. Mở project trong **Unity 2022.3.x** — Unity Package Manager sẽ tự pull `UniTask` từ git URL trong `manifest.json`.
2. Đợi compile xong (~30 giây nếu chưa cache).
3. Menu **HDG → Arrow → Create Test Scene** → tự tạo `Assets/GameAssets/Scenes/ArrowTest.unity`.
4. Nhấn **Play** → click vào head các mũi tên → arrow chạy.

## Cấu trúc

```
Assets/GameAssets/Scripts/Gameplay/Arrows/
├─ Model/                # POCO state
│  ├─ Arrow.cs           # core model (tiles, color, state, events)
│  ├─ ArrowDto.cs        # JSON deserialization target
│  ├─ LevelDto.cs
│  ├─ Direction.cs       # enum + extension methods
│  ├─ ColorType.cs       # Random / Custom / Any
│  └─ DestroyReason.cs   # Click / Hammer / Magnet / Blade
│
├─ Path/                 # Path planning
│  ├─ ArrowPath.cs       # list of (Tile, Direction) steps
│  ├─ ArrowCollision.cs  # collision data (initiator, blocker, tile)
│  └─ ArrowPathBuilder.cs# walks from head outward, detects collision
│
├─ Controller/           # Logic
│  ├─ DotsController.cs  # cell ↔ world conversion
│  ├─ ArrowsController.cs# manages all arrows on field
│  ├─ ArrowGameController.cs# extends HDG BaseGameController
│  ├─ ArrowLevel.cs      # extends HDG BaseLevel (per-level data prefab)
│  └─ I*.cs              # interfaces
│
├─ View/                 # Rendering
│  ├─ LineRendererArrowView.cs  # main view: LineRenderer + Head sprite
│  ├─ ArrowMeshHandler.cs       # bakes MeshCollider from LineRenderer
│  └─ ArrowViewFactory.cs       # creates view (programmatic or from prefab)
│
├─ Input/
│  ├─ InputServiceEvents.cs# raw click/drag/hold/UI-overlap detection
│  └─ ArrowInputAssist.cs  # 3-layer pick: raycast + neighbor + best
│
├─ Lose/
│  ├─ LoseConditionType.cs        # Steps / Time / Stars enum
│  ├─ BaseLoseCondition.cs        # abstract
│  ├─ LoseConditionSteps.cs       # implementation: steps run out
│  └─ LoseConditionsController.cs # holds active conditions, fires Win/Lose
│
├─ Loader/
│  ├─ JsonLevelParser.cs   # parse JSON, hex colors, index→tile
│  └─ ArrowLevelLoader.cs  # MonoBehaviour: load Resources/Levels/{name}
│
├─ Setup/
│  └─ ArrowSceneAutoSetup.cs # 1-component scene builder (no prefabs needed)
│
└─ Editor/
   └─ ArrowTestSceneCreator.cs # menu HDG/Arrow/Create Test Scene
```

## Pipeline tóm tắt

```
                 ┌─────────────────────────────────────────────┐
                 │           ArrowGameController                │
                 │           : BaseGameController               │
                 └─────────────────┬────────────────────────────┘
                                   │ on EventID.StartGame
                                   ▼
                          LevelLoader (HDG)
                                   │ Instantiate ArrowLevel prefab
                                   ▼
            ┌────────────┐    LevelDto    ┌──────────────────────┐
            │ ArrowLevel ├───────────────►│  ArrowLevelLoader    │
            │ (BaseLevel)│   JsonText     │  parses JSON         │
            └────────────┘                └─────────┬────────────┘
                                                    │ for each ArrowDto
                                                    ▼
                          ┌───────────────────────────────────┐
                          │       ArrowsController            │
                          │   AddArrow(tiles, color, ...)     │
                          │     ↓                             │
                          │   ArrowViewFactory.CreateView()   │
                          │     ↓                             │
                          │   LineRendererArrowView.Setup()   │
                          └─────────┬─────────────────────────┘
                                    │ on user click
                                    ▼
              ┌──────────────────────────────────────────────────┐
              │         ArrowInputAssist.OnClicked()             │
              │  1. Raycast ScreenPos → Plane Y=0 → onField      │
              │  2. Physics.RaycastAll on _arrowLayer            │
              │  3. AddGridNeighborsAround(radius=2)             │
              │  4. PickBestByPointPriority(set, point)          │
              │     fallback: PickNearestLockedArrow             │
              └──────────────────────┬───────────────────────────┘
                                     │ best.Arrow.OnClick()
                                     ▼
                          ArrowsController.OnArrowInteraction()
                                     │
                                     ▼
                          ArrowPathBuilder.Build(arrow, ...)
                                     │ may detect collision → locked
                                     ▼
                          view.PlayMoveOut(path)  OR  view.PlayLocked(path)
                                     │ async via UniTask
                                     ▼
                          arrow.MarkCollected() → arrow.Remove()
                                     │
                                     ▼
                          ArrowsController.OnArrowCollected
                                     │
                                     ├──► LoseConditionSteps._clicks++
                                     ├──► IsLevelCompleted? → fires Win
                                     └──► TryUnlockBlockedArrows(this)
```

## Hằng số quan trọng (matches source game)

| Hằng | Giá trị | File | Vai trò |
|---|---|---|---|
| Pick radius (world) | 0.92 | ArrowInputAssist | Cap distance pick |
| Grid neighbor radius | 2 ô | ArrowInputAssist | Vùng quét 5×5 quanh click |
| Move delay | 0.05s | LineRendererArrowView | Trễ trước animate |
| Streak speeds | 35/50/70/95 | LineRendererArrowView | Speed theo streak 1-4+ |
| Locked color | white | LineRendererArrowView | Visual locked |
| LockedMoveCurve | 5%@1.3s → 90%@4min | LineRendererArrowView | Locked di chuyển cực chậm |
| Streak window | 2s | ArrowsController | Reset streak nếu quá |

## Mở rộng

### Thêm Lose Condition mới
1. Inherit `BaseLoseCondition`.
2. Override `ConditionType`, `Value`, `IsLost()`, `SetupValue()`, `AddExtraValue()`.
3. Subscribe events trong `Init()`, unsubscribe trong `DeInit()`.
4. Add vào `LoseConditionsController.Add(yourCondition)`.

### Thay LineRenderer mode bằng Sprite mode
1. Tạo class `SpriteArrowView : MonoBehaviour, IArrowView`.
2. Trong `Setup()`, instantiate sprite GameObjects per tile từ pool.
3. Update `ArrowViewFactory.CreateView()` để chọn view type theo config.

### Thêm Booster (Hammer/Magnet/Hint/Guidelines)
1. Inherit pattern: tách `Controller` (game logic) + `BoosterController` (UI integration).
2. Hammer: subscribe `InputServiceEvents.Clicked`, set flag `_isWaitArrow`, override pick logic.
3. Magnet: gọi `ArrowsController.GetArrowsToRemove(count)`, set `DestroyReason.Magnet`, call `arrow.Remove()`.
4. Hint: iterate `ArrowsController.Arrows`, pick arrow chưa locked, call `view.PlayHint()`.
5. Guidelines: subscribe `OnArrowAdded`, spawn line GameObject per arrow, dispose khi `OnArrowCollected`.

### Tích hợp với HDG framework
- `ArrowGameController` đã extend `BaseGameController` → events `StartGame/EndGame/ReplayGame/CancelGame` hoạt động.
- Khi win/lose, `EventID.EndGame` được post → các UI screens listen sẽ tự reactivate.
- Add `LevelManager : BaseLevel` reference trong scene → `LevelLoader.LoadLevel(int)` sẽ instantiate level prefab từ `Resources/Levels/{path}{level}`.

## Sample levels có sẵn

`Assets/GameAssets/Resources/Levels/`:
- **Level_1_json** — 7×6 với 3 arrows (vàng/xanh/đỏ) — easiest
- **Level_2_json**, **Level_3_json** — easy
- **Level_5_HM**, **Level_10_HM** — medium
- **Level_50_HM**, **Level_100_HM** — harder, có overlap nhiều
- **Level_Block_Preview** — demo Block feature (chưa implement Block logic, sẽ ignore)
- **Level_1_TU**, **Level_3_TU** — tutorial style (color trong suốt, layout đặc biệt)

Đổi level test trong `AutoSetup.TestLevelName` field (Inspector).

## Dependencies

- **UniTask** — auto-installed via `manifest.json` từ git URL `Cysharp/UniTask`.
- Không cần DOTween (dùng `Mathf.Lerp` + `UniTask.Yield` thay thế — lifecycle tương đương).

## Known limitations / TODO

| Feature | Status | Note |
|---|---|---|
| Block / Blade / Pipe features | ❌ chưa | `LevelFeatureData` trong JSON sẽ bị ignore; cần thêm `LevelFeatureController` |
| Combo system | ❌ chưa | Có `_collectedStreak` nhưng không có UI |
| Boosters | ❌ chưa | Pattern đã có trong `IArrowView.PlayHint`, cần thêm controllers |
| Sprite mode | ❌ chưa | Chỉ có LineRenderer mode |
| Dot reveal animation | ❌ chưa | Có thể add vào `LineRendererArrowView.MoveOutAsync` |
| Trail effect | ❌ chưa | Skeleton có sẵn (`AnimationsConfig._arrowAnimations.TrailGradient`) |
| Time / Stars lose conditions | ❌ chưa | Chỉ implement Steps |
| Locked recovery | ⚠️ partial | `TryUnlockBlockedArrows` rebuild path nhưng chưa test edge cases |

## Architecture notes

- **Model/View tách bạch**: `Arrow` không biết gì về Unity; chỉ event-based.
- **Service Locator nhẹ**: `ArrowsController` được pass qua constructor/SerializeField, không có global static.
- **Async**: dùng `UniTask` (giống source game). `CancellationToken` cleanup khi cancel/dispose.
- **No reflection at runtime** trừ `ArrowSceneAutoSetup` (chỉ dùng trong test scene).
- **Sprites runtime-generated**: `ArrowViewFactory` tự gen sprite tam giác cho head nếu không có art asset → demo chạy được ngay.
