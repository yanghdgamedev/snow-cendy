using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single pre-placed instance that moves ITS OWN transform to follow a target.
/// Targets register via <see cref="TargetFollow"/>; the highest-priority one is
/// followed. Only position is changed (never rotation). A dead-zone of radius
/// <see cref="radius"/> keeps it still until the target point drifts past R.
/// </summary>
public class FollowController : MonoBehaviour
{
    public static FollowController Instance { get; private set; }

    [Tooltip("Dead-zone radius R: only move while farther than this from the target point.")]
    [SerializeField] private float radius = 1f;

    [Tooltip("Lerp speed toward the target point (higher = snappier).")]
    [SerializeField] private float speed = 5f;

    private readonly List<TargetFollow> _targets = new();
    private TargetFollow _current;

    public TargetFollow Current => _current;

    private void Awake()
    {
        Instance = this;
        // Pick up targets already enabled in the scene before this awoke
        // (TargetFollow.OnEnable handles ones enabled/spawned afterwards).
        foreach (var target in FindObjectsOfType<TargetFollow>())
        {
            Register(target);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Register(TargetFollow target)
    {
        if (target == null || _targets.Contains(target))
        {
            return;
        }
        _targets.Add(target);
    }

    public void Unregister(TargetFollow target)
    {
        _targets.Remove(target);
        if (_current == target)
        {
            _current = null;
        }
    }

    private void LateUpdate()
    {
        _current = SelectTarget();
        if (_current == null)
        {
            return;
        }

        Vector3 targetPoint = _current.TargetPoint;

        // Dead-zone: stay put while inside R, otherwise ease toward the center.
        if ((transform.position - targetPoint).sqrMagnitude <= radius * radius)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, targetPoint, speed * Time.deltaTime);
    }

    /// <summary>Highest-priority active target; keeps the current one on a tie (anti-thrash).</summary>
    private TargetFollow SelectTarget()
    {
        TargetFollow best = null;
        int bestPriority = int.MinValue;

        for (int i = 0; i < _targets.Count; i++)
        {
            var target = _targets[i];
            if (target == null || !target.isActiveAndEnabled)
            {
                continue;
            }
            if (target.Priority > bestPriority)
            {
                best = target;
                bestPriority = target.Priority;
            }
        }

        // Tie on the top priority → keep the target we are already following.
        if (_current != null && _current.isActiveAndEnabled && _current.Priority == bestPriority)
        {
            return _current;
        }
        return best;
    }
}
