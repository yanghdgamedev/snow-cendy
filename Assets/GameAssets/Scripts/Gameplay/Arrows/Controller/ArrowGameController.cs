using HDG.BaseGameController;
using HDG.EventDispatcher;
using HDG.Gameplay.Arrows.Loader;
using HDG.Gameplay.Arrows.Lose;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    /// <summary>
    /// HDG-aware controller wiring up the Arrow gameplay flow.
    /// Lifecycle:
    ///   StartGame → load JSON → spawn arrows → install lose condition → wait for win/lose
    ///   On win/lose → fire EndGame (HDG framework handles flow continuation).
    /// </summary>
    [RequireComponent(typeof(LevelLoader))]
    public class ArrowGameController : BaseGameController.BaseGameController
    {
        [Header("Scene refs")]
        [SerializeField] private ArrowsController _arrowsController;
        [SerializeField] private ArrowLevelLoader _arrowLevelLoader;
        [SerializeField] private LoseConditionsController _loseController;

        protected override void OnEnable()
        {
            base.OnEnable();
            levelLoader.RegisterListenerFinishLoad(OnLevelInstantiated);
            levelLoader.RegisterListenerBeginDestroy(OnLevelBeingDestroyed);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (levelLoader != null)
            {
                levelLoader.RemoveListenerFinishLoad(OnLevelInstantiated);
                levelLoader.RemoveListenerBeginDestroy(OnLevelBeingDestroyed);
            }
        }

        private void OnLevelInstantiated(object levelObj)
        {
            // Find ArrowLevel under loaded prefab
            var levelGo = GameObject.FindObjectOfType<ArrowLevel>();
            if (levelGo == null)
            {
                Logger.LogError("[ArrowGameController] No ArrowLevel found in instantiated level prefab.");
                return;
            }

            // 1. Load JSON
            if (levelGo.LevelJson != null)
            {
                _arrowLevelLoader.LoadFromJson(levelGo.LevelJson.text);
            }
            else if (!string.IsNullOrEmpty(levelGo.ResourceName))
            {
                _arrowLevelLoader.LoadFromResources(levelGo.ResourceName);
            }
            else
            {
                Logger.LogError("[ArrowGameController] ArrowLevel has neither LevelJson nor ResourceName.");
                return;
            }

            // 2. Setup lose condition
            _loseController.Clear();
            _loseController.Init(_arrowsController);
            _loseController.Add(new LoseConditionSteps(_arrowsController));
            _loseController.Setup(LoseConditionType.Steps, levelGo.LoseConditionValue);
            _loseController.OnWin += HandleWin;
            _loseController.OnLose += HandleLose;

            GamePlayData.IsPlaying = true;
            GamePlayData.IsEndGame = false;
            GamePlayData.IsWin = false;
            GamePlayData.IsLose = false;
        }

        private void OnLevelBeingDestroyed(object _)
        {
            _loseController.OnWin -= HandleWin;
            _loseController.OnLose -= HandleLose;
            _loseController.Clear();
            _arrowsController.Clear();
        }

        private void HandleWin()
        {
            GamePlayData.IsWin = true;
            GamePlayData.IsLose = false;
            this.PostEvent(EventID.EndGame);
        }

        private void HandleLose()
        {
            GamePlayData.IsWin = false;
            GamePlayData.IsLose = true;
            this.PostEvent(EventID.EndGame);
        }

        protected override void HandleWinGame()
        {
            Logger.Log("Arrow level WIN");
            // Hook your win UI here (open WinScreen via EventDispatcher / UI manager)
        }

        protected override void HandleLoseGame()
        {
            Logger.Log("Arrow level LOSE");
            // Hook your lose UI here
        }
    }
}
