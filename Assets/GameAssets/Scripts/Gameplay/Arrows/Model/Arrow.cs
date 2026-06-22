using System;
using System.Collections.Generic;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Model
{
    /// <summary>
    /// Pure model: state of an arrow on the grid.
    /// View (LineRendererArrowView) listens to OnInteraction to play animation.
    /// Tiles[0] = TAIL (xuất phát). Tiles[Count-1] = HEAD (đầu user click).
    /// </summary>
    public sealed class Arrow
    {
        private readonly List<Vector2Int> _tiles = new();

        public Color Color;
        public Color AnyColor;
        public ColorType ColorType;
        public int ColorIndex;

        public bool Collected { get; private set; }
        public bool Clicked { get; private set; }
        public bool Disposed { get; private set; }
        public DestroyReason DestroyReason { get; private set; }

        public IReadOnlyList<Vector2Int> Tiles => _tiles;

        public Vector2Int Head => _tiles.Count == 0 ? Vector2Int.zero : _tiles[^1];
        public Vector2Int Tail => _tiles.Count == 0 ? Vector2Int.zero : _tiles[0];

        /// <summary>Direction the head is pointing (computed from last 2 tiles).</summary>
        public Direction HeadDirection
        {
            get
            {
                if (_tiles.Count < 2) return Direction.Up;
                return DirectionExtensions.FromOffset(_tiles[^2], _tiles[^1]);
            }
        }

        public Direction TailDirection
        {
            get
            {
                if (_tiles.Count < 2) return Direction.Up;
                return DirectionExtensions.FromOffset(_tiles[1], _tiles[0]);
            }
        }

        public event Action<Arrow> OnInitializeComplete;
        public event Action<Arrow> OnInteraction; // fired when player clicks (path will be built by listener)
        public event Action<Arrow> OnRemove;
        public event Action<Arrow> OnCollected;

        public Arrow AddTile(Vector2Int tile)
        {
            _tiles.Add(tile);
            return this;
        }

        public void Build()
        {
            OnInitializeComplete?.Invoke(this);
        }

        public void OnClick()
        {
            if (Collected || Disposed || Clicked) return;
            Clicked = true;
            OnInteraction?.Invoke(this);
        }

        public void MarkCollected()
        {
            Collected = true;
            OnCollected?.Invoke(this);
        }

        public void Remove()
        {
            OnRemove?.Invoke(this);
        }

        public void SetDestroyReason(DestroyReason reason)
        {
            DestroyReason = reason;
        }

        public void Dispose()
        {
            Disposed = true;
            OnInitializeComplete = null;
            OnInteraction = null;
            OnRemove = null;
            OnCollected = null;
        }

        // For revive / restart: reset state without rebuilding
        public void ResetState()
        {
            Collected = false;
            Clicked = false;
        }
    }
}
