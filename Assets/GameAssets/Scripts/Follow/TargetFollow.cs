using UnityEngine;

/// <summary>
/// Put on any object that wants to be followed. Registers itself with the
/// <see cref="FollowController"/> singleton while enabled and carries its own
/// follow offset + priority. The controller follows the registered target with
/// the highest priority.
/// </summary>
public class TargetFollow : MonoBehaviour
{
    [Tooltip("Higher value = higher priority. On a tie the currently-followed target is kept.")]
    [SerializeField] private int priority;

    [Tooltip("Offset added to this object's position to get the point the follower aims at.")]
    [SerializeField] private Vector3 offset;

    public int Priority => priority;
    public Vector3 Offset => offset;

    /// <summary>World point the follower should move toward for this target.</summary>
    public Vector3 TargetPoint => transform.position + offset;

    private void OnEnable()
    {
        if (FollowController.Instance != null)
        {
            FollowController.Instance.Register(this);
        }
    }

    private void OnDisable()
    {
        if (FollowController.Instance != null)
        {
            FollowController.Instance.Unregister(this);
        }
    }
}
