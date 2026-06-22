using HDG.Gameplay.Arrows.Controller;

namespace HDG.Gameplay.Arrows.Lose
{
    /// <summary>
    /// Counts player clicks. When clicks reach Value → IsLost = true.
    /// Locked clicks DO NOT count (matches InfinityMissedClick=1 behavior).
    /// </summary>
    public class LoseConditionSteps : BaseLoseCondition
    {
        private readonly IArrowsController _arrowsController;
        private int _clicks;
        private int _max;

        public override LoseConditionType ConditionType => LoseConditionType.Steps;
        public override int Value => _max;
        public int Clicks => _clicks;
        public int Remaining => _max - _clicks;

        public LoseConditionSteps(IArrowsController arrowsController)
        {
            _arrowsController = arrowsController;
        }

        public override void SetupValue(int value)
        {
            _max = value;
            _clicks = 0;
            RaiseChanged(false);
        }

        public override void Init()
        {
            _arrowsController.OnArrowCollected += OnArrowCollected;
        }

        public override void DeInit()
        {
            _arrowsController.OnArrowCollected -= OnArrowCollected;
        }

        public override void AddExtraValue(int delta)
        {
            _max += delta;
            RaiseChanged(true);
        }

        private void OnArrowCollected(HDG.Gameplay.Arrows.Model.Arrow arrow)
        {
            // Each successful collection consumes one step.
            _clicks++;
            RaiseChanged(true);
        }

        public override bool IsLost() => _clicks >= _max;
    }
}
