using UnityEngine;

[CreateAssetMenu(fileName = "Button preset", menuName = "ScriptableObjects/Button Preset")]
public class ButtonPreset : ScriptableObject
{
    public float scaleDown = 0.8f;

    public float duration = 0.2f;

    public AnimationCurve curve;
}
