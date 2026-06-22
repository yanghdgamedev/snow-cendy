using UnityEngine;

/// <summary>
/// Joystick-backed input source for the player. Reads direction from the
/// <see cref="InputManager"/> singleton and maps the joystick's 2D vector onto
/// the X/Z movement plane. Sits on the Player GameObject as the BaseInput that
/// BasePlayerMovement consumes.
/// </summary>
public class InputController : BaseInput
{
    protected override Vector3 HandleInput()
    {
        if (InputManager.Instance == null)
        {
            return Vector3.zero;
        }

        Vector2 dir = InputManager.Instance.Direction;
        return new Vector3(dir.x, 0f, dir.y);
    }
}
