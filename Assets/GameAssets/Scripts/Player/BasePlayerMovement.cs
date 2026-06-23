using UnityEngine;

/// <summary>
/// CharacterController-based movement with manual gravity (no Rigidbody).
/// Each frame: refresh grounded state → apply gravity → handle jump → read the
/// input direction → move. Horizontal speed = input * speed * speedMultiplier.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public abstract class BasePlayerMovement : BaseMovement
{
    [SerializeField] protected float speed;
    [Tooltip("How fast the heading turns toward the input direction (degrees/second).")]
    [SerializeField] protected float turnSpeed = 540f;
    [SerializeField] protected float Gravity = 9.8f;
    [SerializeField] protected float GravityOnGround = 15f;
    [SerializeField] protected Vector3 Velocity;

    protected CharacterController _characterController;
    protected Vector3 _tempDirection;
    protected Vector3 _currentDirection;
    protected BaseInput _input;

    protected bool isGrounded = true;
    protected bool lastFrameIsGrounded;

    public override bool IsMoving { get; }

    public override bool IsGrounded => isGrounded;

    public override Vector3 GetDirection()
    {
        HandleDirection();
        return Velocity;
    }

    protected virtual void Awake()
    {
        _input = GetComponent<BaseInput>();
        _characterController = GetComponent<CharacterController>();

        // Start the heading from where we already face (flattened) so the first
        // input doesn't snap from a zero vector.
        _currentDirection = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (_currentDirection.sqrMagnitude < 0.0001f)
        {
            _currentDirection = Vector3.forward;
        }
    }

    protected virtual void Update()
    {
        if (Time.timeScale == 0)
        {
            return;
        }
        HandleGround();
        HandleGravity();
        HandleJump();
        HandleDirection();
        ApplyMovement();
    }

    private void HandleGround()
    {
        lastFrameIsGrounded = isGrounded;
        isGrounded = _characterController.isGrounded;
    }

    private void HandleJump()
    {
        // Jump not implemented yet (kept as an override point).
    }

    private void HandleGravity()
    {
        if (isGrounded)
        {
            Velocity.y = -GravityOnGround;
            if (!lastFrameIsGrounded)
            {
                OnLanding?.Invoke();
            }
        }
        else
        {
            if (lastFrameIsGrounded)
            {
                OnLeaveGround?.Invoke();
            }
            Velocity.y -= Gravity * Time.deltaTime;
        }
    }

    private void HandleDirection()
    {
        _tempDirection = _input.GetInput();
        _tempDirection.y = 0f;

        if (_tempDirection.sqrMagnitude > 0.0001f)
        {
            // Turn the current heading toward the input at a fixed angular speed, so a
            // sharp stick flick curves around smoothly instead of snapping instantly.
            Vector3 target = _tempDirection.normalized;
            float maxRadians = turnSpeed * Mathf.Deg2Rad * Time.deltaTime;
            _currentDirection = Vector3.RotateTowards(_currentDirection, target, maxRadians, 0f);

            Velocity.x = _currentDirection.x * speed * speedMultiplier;
            Velocity.z = _currentDirection.z * speed * speedMultiplier;
        }
        else
        {
            // No input: stop moving, but keep the heading so resuming doesn't snap.
            Velocity.x = 0f;
            Velocity.z = 0f;
        }
    }

    private void ApplyMovement()
    {
        _characterController.Move(Velocity * Time.deltaTime);
    }
}
