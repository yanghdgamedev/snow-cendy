using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Input;
using HDG.Gameplay.Arrows.Loader;
using HDG.Gameplay.Arrows.Lose;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Setup
{
    /// <summary>
    /// Drop into an empty scene; wires up an entire playable Arrow gameplay
    /// from scratch on Awake. No prefabs required.
    ///
    /// Usage:
    /// 1. Create new scene "ArrowTest".
    /// 2. Add empty GameObject "AutoSetup" → add this script.
    /// 3. Set _testLevelName (e.g. "Level_1_json").
    /// 4. Place a JSON level under Resources/Levels/Level_1_json.json (text asset).
    /// 5. Press Play.
    /// </summary>
    public class ArrowSceneAutoSetup : MonoBehaviour
    {
        [Header("Test level (JSON file under Resources/Levels)")]
        [SerializeField] private string _testLevelName = "Level_1_json";

        [Header("Camera")]
        [SerializeField] private float _cameraOrthoSize = 6f;
        [SerializeField] private Color _backgroundColor = new(0.07f, 0.09f, 0.12f);

        [Header("Arrow rendering")]
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private float _arrowWidth = 0.5f;

        [Header("Lose condition")]
        [SerializeField] private LoseConditionType _loseType = LoseConditionType.Steps;
        [SerializeField] private int _loseValue = 5;

        private void Awake()
        {
            BuildScene();
        }

        private void BuildScene()
        {
            // 1. Camera (top-down ortho)
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                cam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            cam.transform.position = new Vector3(0f, 10f, 0f);
            cam.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            cam.orthographic = true;
            cam.orthographicSize = _cameraOrthoSize;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = _backgroundColor;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;

            // 2. Ensure ArrowLayer exists (use Default if not)
            int arrowLayer = LayerMask.NameToLayer("ArrowLayer");
            if (arrowLayer < 0) arrowLayer = 0; // Default

            // 3. Grid root
            var gridRoot = new GameObject("GridRoot").transform;

            // 4. DotsController
            var dotsGo = new GameObject("DotsController");
            dotsGo.transform.SetParent(gridRoot, false);
            var dots = dotsGo.AddComponent<DotsController>();
            // assign via reflection-free helper
            SetPrivateField(dots, "_cellSize", _cellSize);
            SetPrivateField(dots, "_gridRoot", gridRoot);

            // 5. ArrowsController + ViewFactory
            var arrowsGo = new GameObject("ArrowsController");
            arrowsGo.transform.SetParent(gridRoot, false);
            var arrows = arrowsGo.AddComponent<ArrowsController>();
            var factory = arrowsGo.AddComponent<View.ArrowViewFactory>();
            SetPrivateField(arrows, "_container", arrowsGo.transform);
            SetPrivateField(arrows, "_dotsController", dots);
            SetPrivateField(arrows, "_viewFactory", factory);
            SetPrivateField(factory, "_arrowWidth", _arrowWidth);
            SetPrivateField(factory, "_arrowLayer", (LayerMask)(1 << arrowLayer));

            // 6. Loader
            var loaderGo = new GameObject("ArrowLevelLoader");
            loaderGo.transform.SetParent(gridRoot, false);
            var loader = loaderGo.AddComponent<ArrowLevelLoader>();
            SetPrivateField(loader, "_arrowsController", arrows);
            SetPrivateField(loader, "_resourcesFolder", "Levels");

            // 7. Input
            var inputGo = new GameObject("Input");
            var input = inputGo.AddComponent<InputServiceEvents>();
            var assist = inputGo.AddComponent<ArrowInputAssist>();
            SetPrivateField(assist, "_arrowLayer", (LayerMask)(1 << arrowLayer));
            SetPrivateField(assist, "_camera", cam);
            SetPrivateField(assist, "_input", input);
            SetPrivateField(assist, "_arrowsController", arrows);
            SetPrivateField(assist, "_dotsController", dots);

            // 8. Lose condition
            var loseGo = new GameObject("LoseConditions");
            var loseCtrl = loseGo.AddComponent<LoseConditionsController>();
            loseCtrl.Init(arrows);

            // 9. Load level
            var dto = loader.LoadFromResources(_testLevelName);
            if (dto == null)
            {
                Debug.LogError($"[ArrowSceneAutoSetup] Could not load level '{_testLevelName}'. " +
                               $"Place '{_testLevelName}.json' under Resources/Levels/.");
                return;
            }

            // 10. Configure lose
            loseCtrl.Add(new LoseConditionSteps(arrows));
            loseCtrl.Setup(_loseType, _loseValue);
            loseCtrl.OnWin += () => Debug.Log("<color=lime>WIN</color>");
            loseCtrl.OnLose += () => Debug.Log("<color=red>LOSE</color>");

            // 11. Auto-fit camera so the field fills the view nicely
            float maxDim = Mathf.Max(dto.XSize, dto.YSize);
            cam.orthographicSize = (maxDim * _cellSize * 0.5f) + 1f;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field == null)
            {
                Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
                return;
            }
            field.SetValue(target, value);
        }
    }
}
