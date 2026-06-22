using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Path
{
    /// <summary>
    /// Records a collision between an arrow's intended path and another arrow.
    /// </summary>
    public struct ArrowCollision
    {
        public readonly Arrow Initiator;
        public Arrow Blocker;
        public Vector2Int? CollisionTile;

        public bool HasCollision => Blocker != null && CollisionTile.HasValue;

        public ArrowCollision(Arrow initiator)
        {
            Initiator = initiator;
            Blocker = null;
            CollisionTile = null;
        }

        public void SetCollision(Vector2Int tile, Arrow blocker)
        {
            CollisionTile = tile;
            Blocker = blocker;
        }
    }
}
