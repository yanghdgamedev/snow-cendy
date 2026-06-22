using System;
using UnityEngine;

/// <summary>
/// Base for any moving character (player, enemy, AI). Exposes movement-state
/// events and a speed multiplier so gameplay systems (boosters, slow zones…)
/// can affect movement without knowing the concrete movement type.
/// </summary>
public abstract class BaseMovement : MonoBehaviour
{
    public virtual bool IsGrounded { get; set; }
    public abstract bool IsMoving { get; }

    public Action OnStop;
    public Action OnRun;
    public Action OnLeaveGround;
    public Action OnLanding;

    protected float speedMultiplier = 1f;

    public virtual void ScaleSpeed(float scale)
    {
    }

    public abstract Vector3 GetDirection();

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
}
