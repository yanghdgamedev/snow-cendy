using System;
using System.Collections.Generic;
using HDG.Gameplay.Arrows.Controller;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Lose
{
    /// <summary>
    /// Holds the active lose conditions for the current level.
    /// Listens to ArrowsController events to detect win/lose.
    /// </summary>
    public class LoseConditionsController : MonoBehaviour
    {
        public event Action OnWin;
        public event Action OnLose;
        public event Action<bool> OnConditionChanged;

        private IArrowsController _arrowsController;
        private readonly List<BaseLoseCondition> _conditions = new();
        private bool _completed;

        public IReadOnlyList<BaseLoseCondition> Conditions => _conditions;

        public void Init(IArrowsController arrowsController)
        {
            _arrowsController = arrowsController;
            _arrowsController.OnArrowCollected += OnArrowCollected;
        }

        public void Add(BaseLoseCondition condition)
        {
            _conditions.Add(condition);
            condition.Init();
            condition.OnChanged += OnAnyConditionChanged;
        }

        public void Setup(LoseConditionType type, int value)
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].ConditionType == type)
                    _conditions[i].SetupValue(value);
            }
        }

        public void Clear()
        {
            for (int i = 0; i < _conditions.Count; i++)
            {
                _conditions[i].OnChanged -= OnAnyConditionChanged;
                _conditions[i].DeInit();
            }
            _conditions.Clear();
            if (_arrowsController != null)
            {
                _arrowsController.OnArrowCollected -= OnArrowCollected;
                _arrowsController = null;
            }
            _completed = false;
        }

        private void OnAnyConditionChanged(bool withAnimation)
        {
            OnConditionChanged?.Invoke(withAnimation);
            if (_completed) return;

            for (int i = 0; i < _conditions.Count; i++)
            {
                if (_conditions[i].IsLost())
                {
                    _completed = true;
                    OnLose?.Invoke();
                    return;
                }
            }
        }

        private void OnArrowCollected(HDG.Gameplay.Arrows.Model.Arrow _)
        {
            if (_completed) return;
            if (_arrowsController != null && _arrowsController.IsLevelCompleted)
            {
                _completed = true;
                OnWin?.Invoke();
            }
        }
    }
}
