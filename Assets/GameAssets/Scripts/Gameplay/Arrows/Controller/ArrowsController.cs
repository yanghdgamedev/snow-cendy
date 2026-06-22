using System;
using System.Collections.Generic;
using HDG.Gameplay.Arrows.Model;
using HDG.Gameplay.Arrows.Path;
using HDG.Gameplay.Arrows.View;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    /// <summary>
    /// Owns all arrows on the field. Manages tile↔arrow lookup, collected streak,
    /// and dispatches events for game-flow listeners (LoseCondition, Combo, GuideLines).
    /// </summary>
    public class ArrowsController : MonoBehaviour, IArrowsController
    {
        [SerializeField] private Transform _container;
        [SerializeField] private DotsController _dotsController;
        [SerializeField] private ArrowViewFactory _viewFactory;
        [SerializeField] private float _streakWindowSeconds = 2f;

        private readonly List<Arrow> _arrows = new();
        private readonly Dictionary<Vector2Int, Arrow> _arrowsByTile = new();
        private readonly Dictionary<Arrow, IArrowView> _views = new();

        private Vector2Int _fieldSize;
        private int _collectedStreak;
        private float _lastCollectionTime = -999f;
        private int _farmed;

        public IReadOnlyList<Arrow> Arrows => _arrows;
        public int ArrowsLeft => _arrows.Count - _farmed;
        public Vector2Int FieldSize => _fieldSize;
        public bool IsLevelCompleted => _arrows.Count > 0 && _farmed >= _arrows.Count;

        public event Action<Arrow> OnArrowAdded;
        public event Action<Arrow> OnArrowCollected;
        public event Action<ArrowCollision> OnLockedClick;
        public event Action OnInitialized;

        public Transform Container => _container != null ? _container : transform;

        public void SetFieldSize(Vector2Int size)
        {
            _fieldSize = size;
            _dotsController.SetFieldSize(size);
        }

        public Arrow AddArrow(IReadOnlyList<Vector2Int> tiles, Color color,
                              ColorType colorType = ColorType.Random, int colorIndex = 0)
        {
            var arrow = new Arrow
            {
                Color = color,
                ColorType = colorType,
                ColorIndex = colorIndex,
            };
            for (int i = 0; i < tiles.Count; i++)
            {
                arrow.AddTile(tiles[i]);
                _arrowsByTile[tiles[i]] = arrow;
            }

            arrow.OnInteraction += OnArrowInteraction;
            arrow.OnCollected += OnArrowCollectedInternal;
            arrow.OnRemove += OnArrowRemove;

            arrow.Build();

            _arrows.Add(arrow);

            // Spawn view
            var view = _viewFactory.CreateView(Container, arrow, _dotsController, this);
            _views[arrow] = view;

            OnArrowAdded?.Invoke(arrow);
            return arrow;
        }

        public void NotifyInitialized()
        {
            OnInitialized?.Invoke();
        }

        public bool TryGetArrow(Vector2Int tile, out Arrow arrow, bool includeCollected = false)
        {
            if (_arrowsByTile.TryGetValue(tile, out arrow))
            {
                if (!includeCollected && arrow.Collected) { arrow = null; return false; }
                return true;
            }
            return false;
        }

        public IArrowView GetView(Arrow arrow) =>
            _views.TryGetValue(arrow, out var view) ? view : null;

        public Color GetArrowColor(Arrow arrow) => arrow.Color;

        public int GetCollectedStreak() => _collectedStreak;

        private void OnArrowInteraction(Arrow arrow)
        {
            // Build path with collision detection
            var path = ArrowPathBuilder.Build(arrow, _fieldSize, this);

            var view = GetView(arrow);
            if (view == null) return;

            if (path.Locked)
            {
                // Locked click — emit event so penalty systems can react
                OnLockedClick?.Invoke(path.Collision);
                view.PlayLocked(path);
            }
            else
            {
                view.PlayMoveOut(path);
            }
        }

        private void OnArrowCollectedInternal(Arrow arrow)
        {
            // Update streak
            float now = Time.time;
            if (now - _lastCollectionTime <= _streakWindowSeconds) _collectedStreak++;
            else _collectedStreak = 1;
            _lastCollectionTime = now;

            // Free occupied tiles
            for (int i = 0; i < arrow.Tiles.Count; i++)
            {
                if (_arrowsByTile.TryGetValue(arrow.Tiles[i], out var a) && a == arrow)
                    _arrowsByTile.Remove(arrow.Tiles[i]);
            }

            _farmed++;
            OnArrowCollected?.Invoke(arrow);

            // Try to unlock arrows that were locked because of this one
            TryUnlockBlockedArrows(arrow);
        }

        private void TryUnlockBlockedArrows(Arrow justCollected)
        {
            for (int i = 0; i < _arrows.Count; i++)
            {
                var a = _arrows[i];
                if (a.Collected || a.Disposed || !a.Clicked) continue;
                var view = GetView(a);
                if (view == null || !view.IsLocked) continue;

                // Re-attempt: synthesize a click without changing Clicked flag.
                var newPath = ArrowPathBuilder.Build(a, _fieldSize, this);
                if (!newPath.Locked)
                {
                    view.PlayMoveOut(newPath);
                }
            }
        }

        private void OnArrowRemove(Arrow arrow)
        {
            for (int i = 0; i < arrow.Tiles.Count; i++)
            {
                if (_arrowsByTile.TryGetValue(arrow.Tiles[i], out var a) && a == arrow)
                    _arrowsByTile.Remove(arrow.Tiles[i]);
            }

            if (_views.TryGetValue(arrow, out var view))
            {
                view.Dispose();
                _views.Remove(arrow);
            }

            arrow.OnInteraction -= OnArrowInteraction;
            arrow.OnCollected -= OnArrowCollectedInternal;
            arrow.OnRemove -= OnArrowRemove;
            arrow.Dispose();
        }

        public void Clear()
        {
            // Snapshot to avoid mutation while iterating
            var snapshot = new List<Arrow>(_arrows);
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot[i].Remove();
            }
            _arrows.Clear();
            _arrowsByTile.Clear();
            _views.Clear();
            _farmed = 0;
            _collectedStreak = 0;
        }
    }
}
