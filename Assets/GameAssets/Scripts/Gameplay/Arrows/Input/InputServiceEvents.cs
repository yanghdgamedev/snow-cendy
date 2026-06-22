using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HDG.Gameplay.Arrows.Input
{
    /// <summary>
    /// Lightweight pointer/touch wrapper. Distinguishes click vs drag vs hold,
    /// and swallows clicks that land on UI.
    /// </summary>
    public class InputServiceEvents : MonoBehaviour
    {
        [Header("Click vs Drag")]
        [SerializeField] private float _clickMaxDuration = 0.5f;
        [SerializeField] private float _clickTolerancePx = 20f;
        [SerializeField] private float _dragStartPx = 30f;
        [SerializeField] private float _holdThreshold = 0.6f;

        private bool _isPressed;
        private bool _dragStarted;
        private bool _holdSent;
        private Vector2 _downPos;
        private float _pressStart;

        public event Action<Vector2> Clicked;
        public event Action<Vector2> PointerDragged;
        public event Action<Vector2> HoldStarted;
        public event Action<Vector2> HoldEnded;

        private void Update()
        {
            HandleMouseAndTouch();
        }

        private void HandleMouseAndTouch()
        {
            // Unity Old Input System (works with both mouse and touch fallback)
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                _downPos = (Vector2)UnityEngine.Input.mousePosition;
                if (IsOverUI(_downPos)) return;
                _isPressed = true;
                _dragStarted = false;
                _holdSent = false;
                _pressStart = Time.unscaledTime;
            }
            else if (_isPressed && UnityEngine.Input.GetMouseButton(0))
            {
                Vector2 cur = (Vector2)UnityEngine.Input.mousePosition;
                float distSqr = (cur - _downPos).sqrMagnitude;
                if (!_dragStarted && distSqr > _dragStartPx * _dragStartPx)
                {
                    _dragStarted = true;
                }
                if (_dragStarted) PointerDragged?.Invoke(cur);

                if (!_holdSent && !_dragStarted && Time.unscaledTime - _pressStart >= _holdThreshold)
                {
                    _holdSent = true;
                    HoldStarted?.Invoke(_downPos);
                }
            }
            else if (_isPressed && UnityEngine.Input.GetMouseButtonUp(0))
            {
                Vector2 cur = (Vector2)UnityEngine.Input.mousePosition;
                bool inTime = (Time.unscaledTime - _pressStart) <= _clickMaxDuration;
                bool inTolerance = (cur - _downPos).sqrMagnitude <= _clickTolerancePx * _clickTolerancePx;
                if (_holdSent) HoldEnded?.Invoke(cur);
                if (!_dragStarted && inTime && inTolerance && !_holdSent)
                {
                    Clicked?.Invoke(cur);
                }
                _isPressed = false;
            }
        }

        private bool IsOverUI(Vector2 screenPos)
        {
            if (EventSystem.current == null) return false;
            var ped = new PointerEventData(EventSystem.current) { position = screenPos };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(ped, hits);
            return hits.Count > 0;
        }
    }
}
