using System;
using HDG.Gameplay.Arrows.Model;
using HDG.Gameplay.Arrows.Path;
using UnityEngine;

namespace HDG.Gameplay.Arrows.View
{
    public interface IArrowView : IDisposable
    {
        Arrow Arrow { get; }
        Transform Head { get; }
        GameObject GameObject { get; }
        Vector3 Center { get; }
        Color CurrentColor { get; }
        bool IsLocked { get; }

        void Setup(Arrow model);
        void SetTargetColor(Color color);
        void PlayMoveOut(ArrowPath path);
        void PlayLocked(ArrowPath path);
        void PlayHint();
    }
}
