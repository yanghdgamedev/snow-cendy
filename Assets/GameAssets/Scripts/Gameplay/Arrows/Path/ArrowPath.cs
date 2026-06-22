using System.Collections;
using System.Collections.Generic;
using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Path
{
    /// <summary>
    /// Pre-computed path the arrow will traverse when clicked.
    /// Each Step is one tile, with the direction the arrow enters that tile from.
    /// </summary>
    public class ArrowPath : IEnumerable<ArrowPath.Step>
    {
        public readonly struct Step
        {
            public readonly Vector2Int Tile;
            public readonly Direction Direction;

            public Step(Vector2Int tile, Direction direction)
            {
                Tile = tile;
                Direction = direction;
            }
        }

        private readonly List<Step> _steps = new();

        public Arrow Arrow { get; }
        public bool Locked { get; private set; }
        public ArrowCollision Collision { get; private set; }

        public int Length => _steps.Count;
        public bool IsValid => _steps.Count > 0;
        public Step First => _steps[0];
        public Step Last => _steps[^1];
        public Step this[int index] => _steps[index];

        public ArrowPath(Arrow arrow)
        {
            Arrow = arrow;
        }

        public ArrowPath SetState(bool locked)
        {
            Locked = locked;
            return this;
        }

        public ArrowPath SetCollision(ArrowCollision collision)
        {
            Collision = collision;
            return this;
        }

        public void AddStep(Vector2Int tile, Direction direction)
        {
            _steps.Add(new Step(tile, direction));
        }

        public bool Contains(Vector2Int tile)
        {
            for (int i = 0; i < _steps.Count; i++)
            {
                if (_steps[i].Tile == tile) return true;
            }
            return false;
        }

        public IEnumerator<Step> GetEnumerator() => _steps.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _steps.GetEnumerator();
    }
}
