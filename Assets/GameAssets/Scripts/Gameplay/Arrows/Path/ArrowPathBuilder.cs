using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Path
{
    /// <summary>
    /// Builds the path an arrow will traverse when clicked.
    /// Walks tile-by-tile from head outward; stops at field edge or collision.
    /// </summary>
    public static class ArrowPathBuilder
    {
        public static ArrowPath Build(Arrow arrow, Vector2Int fieldSize, IArrowsLookup arrowsLookup)
        {
            var path = new ArrowPath(arrow);
            var direction = arrow.HeadDirection;
            var current = arrow.Head;

            // Step 0: include the head tile itself as starting step
            path.AddStep(current, direction);

            // Walk in direction until out-of-bounds or collision
            // Safety cap: never loop more than fieldSize.x * fieldSize.y steps
            int maxSteps = fieldSize.x * fieldSize.y + 1;
            for (int i = 0; i < maxSteps; i++)
            {
                var offset = direction.GetOffset();
                var next = current + offset;

                // Out of bounds → arrow exits the field cleanly
                if (next.x < 0 || next.x >= fieldSize.x ||
                    next.y < 0 || next.y >= fieldSize.y)
                {
                    return path; // unlocked, valid completion
                }

                // Another arrow occupying next tile (and not the same arrow)
                if (arrowsLookup.TryGetArrow(next, out var blocker)
                    && blocker != arrow
                    && !blocker.Collected)
                {
                    var collision = new ArrowCollision(arrow);
                    collision.SetCollision(next, blocker);
                    path.SetCollision(collision);
                    path.SetState(true); // locked
                    return path;
                }

                path.AddStep(next, direction);
                current = next;
            }

            return path;
        }
    }
}
