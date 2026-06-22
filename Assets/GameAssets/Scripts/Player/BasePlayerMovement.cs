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
    [SerializeField] protected float Gravity = 9.8f;
    [SerializeField] protected float GravityOnGround = 15f;
    [SerializeField] protected Vector3 Velocity;

    protected CharacterController _characterController;
    protected Vector3 _tempDirection;
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
        _tempDirection = _tempDirection.normalized;
        Velocity.x = _tempDirection.x * speed * speedMultiplier;
        Velocity.z = _tempDirection.z * speed * speedMultiplier;
    }

    private void ApplyMovement()
    {
        _characterController.Move(Velocity * Time.deltaTime);
    }
}
