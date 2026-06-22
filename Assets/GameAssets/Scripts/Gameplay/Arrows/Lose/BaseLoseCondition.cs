using System;

namespace HDG.Gameplay.Arrows.Lose
{
    public abstract class BaseLoseCondition
    {
        public abstract LoseConditionType ConditionType { get; }
        public abstract int Value { get; }

        public event Action<bool> OnChanged;

        public abstract bool IsLost();
        public abstract void SetupValue(int value);
        public abstract void AddExtraValue(int delta);

        public virtual void Init() { }
        public virtual void DeInit() { }

        protected void RaiseChanged(bool withAnimation) => OnChanged?.Invoke(withAnimation);
    }
}
