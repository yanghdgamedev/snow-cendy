using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    /// <summary>
    /// Owns the playing field grid: cell↔world conversion + grid root transform.
    /// </summary>
    public interface IDotsController
    {
        float CellSize { get; }
        Transform GridRoot { get; }

        Vector3 CellToWorldCenter(Vector2Int cell);
        bool WorldToCell(Vector3 world, out Vector2Int cell);
    }
}
