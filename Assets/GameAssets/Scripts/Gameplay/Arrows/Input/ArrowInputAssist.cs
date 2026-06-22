using System.Collections.Generic;
using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Model;
using HDG.Gameplay.Arrows.View;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Input
{
    /// <summary>
    /// Three-layer click pipeline:
    /// 1. Project click to a world point on the field plane.
    /// 2. Gather direct hits via Physics.RaycastAll (LayerMask = arrow layer).
    /// 3. Expand candidates by grid neighbor radius around the click point.
    /// 4. Pick best by distance + priority; fallback to nearest locked arrow.
    /// </summary>
    public class ArrowInputAssist : MonoBehaviour
    {
        [SerializeField] private LayerMask _arrowLayer;
        [SerializeField] private Camera _camera;
        [SerializeField] private InputServiceEvents _input;
        [SerializeField] private ArrowsController _arrowsController;
        [SerializeField] private DotsController _dotsController;

        [Header("Pick tuning (matches source InputConfig)")]
        [SerializeField] private float _pickRadiusWorld = 0.92f;
        [SerializeField] private int _gridNeighborRadius = 2;
        [SerializeField] private float _headPriorityDistance = 0.5f;

        private readonly HashSet<IArrowView> _candidates = new();

        private Camera Cam => _camera != null ? _camera : Camera.main;

        private void OnEnable()
        {
            if (_input != null) _input.Clicked += OnClicked;
        }

        private void OnDisable()
        {
            if (_input != null) _input.Clicked -= OnClicked;
        }

        private void OnClicked(Vector2 screenPos)
        {
            var cam = Cam;
            if (cam == null) return;

            if (!TryProjectOnField(cam, screenPos, out Vector3 onField, out float maxRayDist))
                return;

            _candidates.Clear();

            // Layer 1: direct raycast hits
            var ray = cam.ScreenPointToRay(screenPos);
            var hits = Physics.RaycastAll(ray, maxRayDist, _arrowLayer);
            for (int i = 0; i < hits.Length; i++)
            {
                var view = hits[i].collider.GetComponentInParent<IArrowView>();
                if (view != null) _candidates.Add(view);
            }

            // Layer 2: grid neighbor expansion
            AddGridNeighborsAround(onField, _gridNeighborRadius, _candidates);

            // Layer 3: pick best (active arrows only)
            var best = PickBestByPointPriority(_candidates, onField);
            if (best != null)
            {
                best.Arrow.OnClick();
                return;
            }

            // Fallback: nearest locked arrow → triggers locked-click penalty
            var locked = PickNearestLockedArrow(_candidates, onField);
            if (locked != null)
            {
                locked.Arrow.OnClick();
            }
            // else: missed click — do nothing (matches InfinityMissedClick=true behavior)
        }

        private bool TryProjectOnField(Camera cam, Vector2 screenPos,
                                       out Vector3 onField, out float maxRayDist)
        {
            var ray = cam.ScreenPointToRay(screenPos);
            var plane = new Plane(Vector3.up, Vector3.zero); // field on plane Y=0
            if (plane.Raycast(ray, out float dist))
            {
                onField = ray.GetPoint(dist);
                maxRayDist = dist + 1f;
                return true;
            }
            onField = default;
            maxRayDist = 0f;
            return false;
        }

        private void AddGridNeighborsAround(Vector3 worldPoint, int radius, HashSet<IArrowView> bucket)
        {
            if (!_dotsController.WorldToCell(worldPoint, out var center)) return;
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    var tile = center + new Vector2Int(dx, dy);
                    if (_arrowsController.TryGetArrow(tile, out var arrow))
                    {
                        var view = _arrowsController.GetView(arrow);
                        if (view != null) bucket.Add(view);
                    }
                }
            }
        }

        private IArrowView PickBestByPointPriority(HashSet<IArrowView> set, Vector3 point)
        {
            IArrowView best = null;
            float bestScore = float.MinValue;

            foreach (var view in set)
            {
                if (view.Arrow.Collected || view.Arrow.Disposed) continue;
                if (view.IsLocked) continue;

                float dist = DistancePointToArrowTiles(point, view);
                if (dist > _pickRadiusWorld) continue;

                // Boost score if click is near the head
                var headWorld = _dotsController.CellToWorldCenter(view.Arrow.Head);
                float headDist = Vector3.Distance(point, headWorld);
                int headBoost = headDist < _headPriorityDistance ? 100 : 0;
                float score = headBoost - dist;

                if (score > bestScore) { bestScore = score; best = view; }
            }
            return best;
        }

        private IArrowView PickNearestLockedArrow(HashSet<IArrowView> set, Vector3 point)
        {
            IArrowView best = null;
            float bestDist = float.MaxValue;
            foreach (var view in set)
            {
                if (view.Arrow.Collected || view.Arrow.Disposed) continue;
                if (!view.IsLocked) continue;
                float d = DistancePointToArrowTiles(point, view);
                if (d < bestDist) { bestDist = d; best = view; }
            }
            return best;
        }

        private float DistancePointToArrowTiles(Vector3 point, IArrowView view)
        {
            float min = float.MaxValue;
            for (int i = 0; i < view.Arrow.Tiles.Count; i++)
            {
                var w = _dotsController.CellToWorldCenter(view.Arrow.Tiles[i]);
                float d = Vector3.Distance(point, w);
                if (d < min) min = d;
            }
            return min;
        }
    }
}
