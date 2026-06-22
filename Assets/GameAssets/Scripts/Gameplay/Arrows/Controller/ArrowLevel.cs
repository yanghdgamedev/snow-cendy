using HDG.Gameplay.Arrows.Loader;
using HDG.Gameplay.Arrows.Lose;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    /// <summary>
    /// Per-level data prefab. Sits in Resources/Levels/ArrowLevel_{N}.prefab
    /// Configures: which JSON to load, which lose condition + value, etc.
    /// Inherits from project's BaseLevel so HDG LevelLoader can spawn it.
    /// </summary>
    public class ArrowLevel : BaseLevel
    {
        [Header("Level data")]
        [SerializeField] private TextAsset _levelJson;
        [Tooltip("Optional override: if set, loads from Resources/Levels/{name}")]
        [SerializeField] private string _resourceName;

        [Header("Lose condition")]
        [SerializeField] private LoseConditionType _loseConditionType = LoseConditionType.Steps;
        [SerializeField, Min(1)] private int _loseConditionValue = 5;

        public TextAsset LevelJson => _levelJson;
        public string ResourceName => _resourceName;
        public LoseConditionType LoseConditionType => _loseConditionType;
        public int LoseConditionValue => _loseConditionValue;
    }
}
