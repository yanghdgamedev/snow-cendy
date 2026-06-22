using UnityEngine;

/// <summary>
/// Abstracts the input source feeding movement. Concrete sources (keyboard,
/// joystick, AI) only implement <see cref="HandleInput"/>; movement code reads
/// <see cref="GetInput"/> and never depends on where the direction comes from.
/// Returned vector is in world space on the X/Z plane (Y unused).
/// </summary>
public abstract class BaseInput : MonoBehaviour
{
    private Vector3 _inputVector;

    protected abstract Vector3 HandleInput();

    private void UpdateInput()
    {
        _inputVector = HandleInput();
    }

    public Vector3 GetInput()
    {
        UpdateInput();
        return _inputVector;
    }
}
