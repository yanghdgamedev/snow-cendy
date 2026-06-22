using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    public interface IArrowsLookup
    {
        bool TryGetArrow(Vector2Int tile, out Arrow arrow, bool includeCollected = false);
    }
}
