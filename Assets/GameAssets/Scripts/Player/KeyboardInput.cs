using UnityEngine;

/// <summary>
/// Temporary input source for testing in the Editor: WASD / arrow keys mapped to
/// the X/Z plane. Swap for a Joystick-based BaseInput on mobile (just add another
/// BaseInput component — movement code is unchanged).
/// </summary>
public class KeyboardInput : BaseInput
{
    protected override Vector3 HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        return new Vector3(x, 0f, z);
    }
}
