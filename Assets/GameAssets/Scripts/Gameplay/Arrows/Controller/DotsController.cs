using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    /// <summary>
    /// Maps grid cells (Vector2Int x,y) to world positions.
    /// Field lies on plane Y=0; cells are squares of CellSize units, centered on integer coords.
    /// Cell (x,y) world center = gridRoot.position + (x * CellSize, 0, y * CellSize) - centerOffset.
    /// CenterOffset is computed from FieldSize so the field is centered around gridRoot.
    /// </summary>
    public class DotsController : MonoBehaviour, IDotsController
    {
        [SerializeField] private float _cellSize = 1f;
        [SerializeField] private Transform _gridRoot;

        private Vector2Int _fieldSize;
        private Vector3 _centerOffset;

        public float CellSize => _cellSize;
        public Transform GridRoot => _gridRoot != null ? _gridRoot : transform;

        public void SetFieldSize(Vector2Int size)
        {
            _fieldSize = size;
            // Center the field on origin: shift by (-(W-1)/2 * cs, 0, -(H-1)/2 * cs)
            _centerOffset = new Vector3(
                (size.x - 1) * 0.5f * _cellSize,
                0f,
                (size.y - 1) * 0.5f * _cellSize);
        }

        public Vector3 CellToWorldCenter(Vector2Int cell)
        {
            var local = new Vector3(cell.x * _cellSize, 0f, cell.y * _cellSize) - _centerOffset;
            return GridRoot.TransformPoint(local);
        }

        public bool WorldToCell(Vector3 world, out Vector2Int cell)
        {
            var local = GridRoot.InverseTransformPoint(world) + _centerOffset;
            int x = Mathf.RoundToInt(local.x / _cellSize);
            int y = Mathf.RoundToInt(local.z / _cellSize);
            cell = new Vector2Int(x, y);
            return cell.x >= 0 && cell.x < _fieldSize.x
                && cell.y >= 0 && cell.y < _fieldSize.y;
        }
    }
}
