using System.Collections.Generic;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Model
{
    public enum Direction
    {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
    }

    public static class DirectionExtensions
    {
        private static readonly Dictionary<Direction, Vector2Int> Offsets = new()
        {
            { Direction.Up, new Vector2Int(0, 1) },
            { Direction.Down, new Vector2Int(0, -1) },
            { Direction.Left, new Vector2Int(-1, 0) },
            { Direction.Right, new Vector2Int(1, 0) },
        };

        private static readonly Dictionary<Direction, int> Angles = new()
        {
            { Direction.Up, 0 },
            { Direction.Right, -90 },
            { Direction.Down, 180 },
            { Direction.Left, 90 },
        };

        public static Vector2Int GetOffset(this Direction direction) => Offsets[direction];

        public static int GetAngle(this Direction direction) => Angles[direction];

        public static Direction Opposite(this Direction direction) => direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => Direction.Up,
        };

        public static Direction FromOffset(Vector2Int from, Vector2Int to)
        {
            var d = to - from;
            if (d.x > 0) return Direction.Right;
            if (d.x < 0) return Direction.Left;
            if (d.y > 0) return Direction.Up;
            return Direction.Down;
        }
    }
}
