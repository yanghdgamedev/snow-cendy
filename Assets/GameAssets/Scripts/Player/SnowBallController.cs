using System;
using UnityEngine;

/// <summary>
/// A snowball that grows as it travels and visually rolls forward.
///
/// - Growth is distance-based: each frame ΔR = distanceMoved * A(R), where A is a
///   conversion ratio sampled from <see cref="conversionCurve"/> by normalized size
///   (R / maxRadius) and clamped to [minConversion, maxConversion]. Small ball → big A
///   (grows fast), big ball → small A (grows slow).
/// - <see cref="radius"/> is the model's scale value; it is clamped to [0, maxRadius] and
///   the model is hidden while at or below <see cref="minRadius"/>.
/// - Movement is detected by measuring this transform's own position delta (something
///   else — e.g. PlayerMovement on the parent — moves it; this script does not move it).
/// - The model rolls without slipping around its local X axis (roll angle = distance / radius).
///
/// Model placement: SnowBallController is the parent, SnowBallModel the visual child.
/// NOTE: per the chosen option the model is offset FORWARD by radius/2 only (local Y = 0,
/// height handled elsewhere). If instead you want it tangent to ground + back-anchored
/// (center at local (0, R, R)), change <see cref="ApplyRadius"/> accordingly.
/// </summary>
public class SnowBallController : MonoBehaviour
{
    [Header("Model")]
    [SerializeField] private Transform ballModel;

    [Header("Size")]
    [SerializeField] private float radius;
    [SerializeField] private float minRadius = 0.7f; // at/below this the model is hidden
    [SerializeField] private float maxRadius = 5f;    // cannot grow past this

    [Header("Growth — A(R)")]
    [Tooltip("Conversion ratio A sampled by normalized size (radius / maxRadius). " +
             "Left (small ball) should be high, right (big ball) low. ΔR = distance * A.")]
    [SerializeField] private AnimationCurve conversionCurve = AnimationCurve.Linear(0f, 2f, 1f, 0.2f);
    [SerializeField] private float minConversion = 0.2f;
    [SerializeField] private float maxConversion = 2f;

    [Header("Roll")]
    [SerializeField] private bool rollModel = true;

    [Header("Move detection")]
    [SerializeField] private float moveThreshold = 0.0001f;

    private Vector3 _lastPos;
    private bool _isHidden = true;

    public float Radius => radius;
    public float MinRadius => minRadius;
    public float MaxRadius => maxRadius;
    public bool IsHidden => _isHidden;

    /// <summary>Raised whenever the radius changes: (radius, maxRadius). Handy for UI progress.</summary>
    public Action<float, float> OnRadiusChanged;

    private void OnEnable()
    {
        _lastPos = transform.position;
        ApplyRadius();
        // Force a consistent initial visibility (ApplyRadius only toggles on change).
        if (ballModel != null)
        {
            _isHidden = radius <= minRadius;
            ballModel.gameObject.SetActive(!_isHidden);
        }
    }

    private void Update()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(pos, _lastPos);

        if (distance > moveThreshold)
        {
            Grow(distance);
            if (rollModel)
            {
                Roll(distance);
            }
        }

        _lastPos = pos;
    }

    private void Grow(float distance)
    {
        if (radius >= maxRadius)
        {
            return;
        }
        SetRadius(radius + distance * GetConversion());
    }

    /// <summary>A(R): smaller ball → larger A, larger ball → smaller A, clamped to [min, max].</summary>
    private float GetConversion()
    {
        float t = maxRadius > 0f ? Mathf.Clamp01(radius / maxRadius) : 0f;
        float a = conversionCurve.Evaluate(t);
        return Mathf.Clamp(a, minConversion, maxConversion);
    }

    public void SetRadius(float value)
    {
        radius = Mathf.Clamp(value, 0f, maxRadius);
        ApplyRadius();
        OnRadiusChanged?.Invoke(radius, maxRadius);
    }

    private Vector3 UnitOffsetBall = new Vector3(0, 1, 1);
    private void ApplyRadius()
    {
        if (ballModel == null)
        {
            return;
        }

        ballModel.localScale = Vector3.one * radius;
        ballModel.localPosition = UnitOffsetBall * (radius * 0.5f);

        bool shouldHide = radius <= minRadius;
        if (shouldHide != _isHidden)
        {
            _isHidden = shouldHide;
            ballModel.gameObject.SetActive(!_isHidden);
        }
    }

    private void Roll(float distance)
    {
        // Rolling without slipping: roll angle = arcLength / rollingRadius.
        // Visual radius = radius * 0.5 (model scaled to 'radius' as diameter).
        float rollingRadius = radius * 0.5f;
        if (rollingRadius <= 0f)
        {
            return;
        }
        float degrees = distance / rollingRadius * Mathf.Rad2Deg;
        ballModel.localRotation *= Quaternion.Euler(degrees, 0f, 0f);
    }
}
