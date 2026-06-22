using UnityEngine;

/// <summary>
/// Player-specific movement: tracks running/stopped state (firing OnRun/OnStop)
/// and turns the character to face its horizontal velocity.
/// </summary>
public class PlayerMovement : BasePlayerMovement
{
    public Vector3 Direction => Velocity.normalized;

    private bool isMoving;

    public override bool IsMoving => isMoving;

    protected override void Update()
    {
        base.Update();

        bool movingNow = Velocity.x != 0 || Velocity.z != 0;
        if (movingNow != isMoving)
        {
            if (movingNow) OnRun?.Invoke();
            else OnStop?.Invoke();
        }
        isMoving = movingNow;

        // Face horizontal velocity. (Reference only rotated when both x AND z were
        // non-zero, so single-axis movement didn't turn — fixed to any horizontal motion.)
        var horizontal = new Vector3(Velocity.x, 0f, Velocity.z);
        if (horizontal.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(horizontal.normalized);
        }
    }

    private void OnDisable()
    {
        isMoving = false;
    }
}
