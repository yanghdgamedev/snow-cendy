using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Model;
using HDG.Gameplay.Arrows.Path;
using UnityEngine;

namespace HDG.Gameplay.Arrows.View
{
    /// <summary>
    /// Renders an arrow as a LineRenderer body + child Head sprite.
    /// Animates by shrinking the tail end of the LineRenderer toward the head,
    /// then disappears when path is fully consumed.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class LineRendererArrowView : MonoBehaviour, IArrowView
    {
        [Header("References")]
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private SpriteRenderer _headSprite;
        [SerializeField] private Transform _head;
        [SerializeField] private ArrowMeshHandler _meshHandler;

        [Header("Tunables")]
        [SerializeField] private float _moveDelay = 0.05f;
        [SerializeField] private float _baseSpeed = 35f; // streak 1
        [SerializeField] private float[] _streakSpeeds = { 35f, 50f, 70f, 95f };
        [SerializeField] private Color _lockedColor = Color.white;

        // Locked move curve: (timeSeconds, distanceFraction)
        // Mirrors AnimationsConfig.LockedMoveCurve from the source game.
        [SerializeField]
        private AnimationCurve _lockedCurve = new(
            new Keyframe(0f, 0f),
            new Keyframe(1.33f, 0.05f),
            new Keyframe(3.05f, 0.136f),
            new Keyframe(20.84f, 0.40f),
            new Keyframe(56.68f, 0.62f),
            new Keyframe(255.85f, 0.90f));

        private IDotsController _dotsController;
        private IArrowsController _arrowsController;
        private CancellationTokenSource _cts;
        private bool _isLocked;
        private Color _originalColor;

        public Arrow Arrow { get; private set; }
        public Transform Head => _head;
        public GameObject GameObject => gameObject;
        public Vector3 Center => transform.position;
        public Color CurrentColor { get; private set; }
        public bool IsLocked => _isLocked;

        public void Bind(IDotsController dotsController, IArrowsController arrowsController)
        {
            _dotsController = dotsController;
            _arrowsController = arrowsController;
        }

        /// <summary>Wire up references when this view is built programmatically by ArrowViewFactory.</summary>
        public void SetReferences(LineRenderer line, SpriteRenderer headSprite, Transform head, ArrowMeshHandler meshHandler)
        {
            _lineRenderer = line;
            _headSprite = headSprite;
            _head = head;
            _meshHandler = meshHandler;
        }

        public void Setup(Arrow model)
        {
            Arrow = model;

            var points = BuildArrowPoints(model);
            _lineRenderer.positionCount = points.Length;
            _lineRenderer.SetPositions(points);

            _head.position = points[^1];
            _head.rotation = Quaternion.Euler(0f, model.HeadDirection.GetAngle(), 0f);

            _originalColor = model.Color;
            SetTargetColor(model.Color);

            _meshHandler?.Bind(_lineRenderer);
            _meshHandler?.RefreshCollider(Camera.main);
        }

        private Vector3[] BuildArrowPoints(Arrow model)
        {
            var arr = new Vector3[model.Tiles.Count];
            for (int i = 0; i < model.Tiles.Count; i++)
                arr[i] = _dotsController.CellToWorldCenter(model.Tiles[i]);
            return arr;
        }

        public void SetTargetColor(Color color)
        {
            CurrentColor = color;
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            if (_headSprite != null) _headSprite.color = color;
        }

        public void PlayMoveOut(ArrowPath path)
        {
            CancelOngoing();
            _cts = new CancellationTokenSource();
            _isLocked = false;
            SetTargetColor(_originalColor);
            MoveOutAsync(path, _cts.Token).Forget();
        }

        public void PlayLocked(ArrowPath path)
        {
            CancelOngoing();
            _cts = new CancellationTokenSource();
            _isLocked = true;
            SetTargetColor(_lockedColor);
            MoveLockedAsync(path, _cts.Token).Forget();
        }

        public void PlayHint()
        {
            HintAsync(destroyCancellationToken).Forget();
        }

        private async UniTask MoveOutAsync(ArrowPath path, CancellationToken ct)
        {
            _meshHandler?.DisableCollider(); // can't be re-clicked while moving

            await UniTask.Delay(TimeSpan.FromSeconds(_moveDelay), cancellationToken: ct);

            float speed = GetSpeedForStreak(_arrowsController?.GetCollectedStreak() ?? 1);

            // We "consume" the path step-by-step from the tail side.
            // Head world stays at the LAST tile of the original arrow,
            // tail moves along path tile-by-tile until reaching head.
            var stepWorlds = new Vector3[path.Length];
            for (int i = 0; i < path.Length; i++)
                stepWorlds[i] = _dotsController.CellToWorldCenter(path[i].Tile);

            // Original arrow positions (already in lineRenderer)
            int initialPositionCount = _lineRenderer.positionCount;
            var positions = new Vector3[initialPositionCount];
            _lineRenderer.GetPositions(positions);

            // Pre-compute destination positions: shrink down to a single point at head
            // For each step in the path, advance position[0] toward positions[1] in time = (cellSize / speed)
            float cellSize = _dotsController.CellSize;
            float timePerCell = cellSize / speed;

            // Walk through the original tiles (we shrink from the tail end)
            int posCount = positions.Length;
            for (int killIndex = 0; killIndex < posCount - 1 && !ct.IsCancellationRequested; killIndex++)
            {
                Vector3 from = positions[killIndex];
                Vector3 to = positions[killIndex + 1];

                float t = 0f;
                while (t < timePerCell && !ct.IsCancellationRequested)
                {
                    t += Time.deltaTime;
                    float lerp = Mathf.Clamp01(t / timePerCell);
                    var live = new Vector3[posCount - killIndex];
                    live[0] = Vector3.Lerp(from, to, lerp);
                    for (int j = 1; j < live.Length; j++) live[j] = positions[killIndex + j];
                    _lineRenderer.positionCount = live.Length;
                    _lineRenderer.SetPositions(live);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }

            if (ct.IsCancellationRequested) return;

            Arrow.MarkCollected();
            // Optionally play exit FX here
            Arrow.Remove();
        }

        private async UniTask MoveLockedAsync(ArrowPath path, CancellationToken ct)
        {
            // Locked: only animate slightly forward, very slowly per LockedMoveCurve.
            // We move head ONE small step toward the collision tile, then keep crawling.
            if (path.Length < 2) return;

            float cellSize = _dotsController.CellSize;
            // Distance to collision = cellSize (one tile beyond last valid step)
            float maxDistance = cellSize;

            int posCount = _lineRenderer.positionCount;
            var positions = new Vector3[posCount];
            _lineRenderer.GetPositions(positions);

            // We will progressively shrink position[0] up to fraction*cellSize toward positions[1].
            Vector3 from = positions[0];
            Vector3 toward = positions[1];

            float startTime = Time.time;
            // Curve is in absolute seconds — read fraction(t) and lerp
            while (!ct.IsCancellationRequested)
            {
                float elapsed = Time.time - startTime;
                float fraction = Mathf.Clamp01(_lockedCurve.Evaluate(elapsed));
                positions[0] = Vector3.Lerp(from, toward, fraction);
                _lineRenderer.SetPositions(positions);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);

                // If user/blocker resolves the situation, ArrowsController re-issues PlayMoveOut
                // → CancelOngoing aborts this loop.
                if (fraction >= 0.999f) break;
            }
        }

        private async UniTask HintAsync(CancellationToken ct)
        {
            var prev = CurrentColor;
            var hint = Color.white;
            const float dur = 0.75f;
            const float fade = 0.05f;
            float t = 0f;
            while (t < fade && !ct.IsCancellationRequested)
            {
                t += Time.deltaTime;
                SetTargetColor(Color.Lerp(prev, hint, t / fade));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(dur), cancellationToken: ct);
            t = 0f;
            while (t < fade && !ct.IsCancellationRequested)
            {
                t += Time.deltaTime;
                SetTargetColor(Color.Lerp(hint, prev, t / fade));
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            SetTargetColor(prev);
        }

        private float GetSpeedForStreak(int streak)
        {
            if (_streakSpeeds == null || _streakSpeeds.Length == 0) return _baseSpeed;
            int idx = Mathf.Clamp(streak - 1, 0, _streakSpeeds.Length - 1);
            return _streakSpeeds[idx];
        }

        private void CancelOngoing()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }

        public void Dispose()
        {
            CancelOngoing();
            if (this != null && gameObject != null) Destroy(gameObject);
        }
    }
}
