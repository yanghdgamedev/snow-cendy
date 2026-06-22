using System;
using System.Collections.Generic;
using HDG.Gameplay.Arrows.Model;
using HDG.Gameplay.Arrows.Path;
using HDG.Gameplay.Arrows.View;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Controller
{
    public interface IArrowsController : IArrowsLookup
    {
        IReadOnlyList<Arrow> Arrows { get; }
        int ArrowsLeft { get; }
        Vector2Int FieldSize { get; }

        event Action<Arrow> OnArrowAdded;
        event Action<Arrow> OnArrowCollected;
        event Action<ArrowCollision> OnLockedClick;
        event Action OnInitialized;

        IArrowView GetView(Arrow arrow);
        Color GetArrowColor(Arrow arrow);
        int GetCollectedStreak();
        bool IsLevelCompleted { get; }

        void Clear();
    }
}
