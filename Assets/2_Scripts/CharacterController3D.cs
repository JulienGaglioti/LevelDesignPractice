using UnityEngine;
using System;

public class CharacterController3D : MonoBehaviour
{
    [SerializeField] private CharacterStatsSO _stats;
    [SerializeField] private Transform _groundCollisionSphere;
    [SerializeField] private float _collisionCheckRadius;
    [SerializeField] private float _magneticActionCooldown;
    public bool PushUnlocked;
    public bool PullUnlocked;

    private CharacterController _controller;
    private InputManager _input;
    private CharacterMagneticArea _magneticSphere;
    private float _currentSpeed;
    private float _targetRotation = 0.0f;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private Vector3 _targetDirection;
    private Vector3 _slopeDirection;
    private RaycastHit _dashHit;
    private RaycastHit _groundHit;
    private bool _onSlope;

    private float _frameLeftGrounded = float.MinValue;
    public bool _grounded;
    private bool _jumpToConsume;
    private bool _bufferedJumpUsable;
    private bool _endedJumpEarly;
    private bool _coyoteUsable;
    private float _timeJumpWasPressed;
    private bool _newJumpPress = true;
    private bool HasBufferedJump => _bufferedJumpUsable && Time.time < _timeJumpWasPressed + _stats.JumpBuffer;
    private bool CanUseCoyote => _coyoteUsable && !_grounded && Time.time < _frameLeftGrounded + _stats.CoyoteTime;


    private bool _magneticActionToConsume;
    private float _magneticActionCooldownTimer;
    private bool _magneticPull;

    private bool _dashing;
    private bool _dashSlowdown;
    private float _timeStartedDash;
    private Vector3 _dashDirection = Vector3.zero;
    private float _dashSlowdownTimer;
    private Transform _dashObject;

    public event Action<bool, float> GroundedChanged;
    public event Action Jumped;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputManager>();
        _magneticSphere = GetComponentInChildren<CharacterMagneticArea>();
        _dashSlowdownTimer = _stats.DashSlowdownDuration;
        _dashDirection = Vector3.zero;
    }
    private void Update()
    {
        GatherInput();
    }
    private void FixedUpdate()
    {
        HandleTimers();
        CheckCollisions();
        HandleJumpInput();
        HandleDash();
        HandleDirection();
        HandleGravity();

        ApplyMovement();
    }

    private void GatherInput()
    {
        if (_input.Jump && _newJumpPress)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = Time.time;
            _newJumpPress = false;
        }

        if (PushUnlocked && _input.Action1 && _magneticActionCooldownTimer >= _magneticActionCooldown)
        {
            _magneticPull = false;
            ExecuteMagneticAction();
        }
        else if (PullUnlocked && _input.Action2 && _magneticActionCooldownTimer >= _magneticActionCooldown)
        {
            _magneticPull = true;
            ExecuteMagneticAction();
        }

        if (!_input.Jump)
        {
            _newJumpPress = true;
        }
    }
    private void HandleTimers()
    {
        if (!_dashing)
        {
            _dashSlowdownTimer += Time.fixedDeltaTime;
            if (_dashSlowdownTimer > _stats.DashSlowdownDuration)
            {
                _dashSlowdown = false;
            }
        }
        _magneticActionCooldownTimer += Time.fixedDeltaTime;
    }
    private void CheckCollisions()
    {
        bool groundHit = false;
        if (Physics.SphereCast(transform.position/*  + Vector3.down * _controller.height / 2 */, _collisionCheckRadius, Vector3.down, out _groundHit, 1.2f, _stats.GroundLayer))
        {
            groundHit = true;

            /* float slopeAngle = Vector3.Angle(_groundHit.normal, Vector3.up);
            if (slopeAngle < _stats.SlopeLimit)
            {
                _slopeDirection = Vector3.zero;
                _onSlope = false;
            }
            else
            {
                _slopeDirection = _groundHit.normal;
                _slopeDirection.y = 0;
                _onSlope = true;
            } */
        }


        // Landed on the Ground
        if (!_grounded && groundHit)
        {
            _grounded = true;
            _coyoteUsable = true;
            _bufferedJumpUsable = true;
            _endedJumpEarly = false;
            GroundedChanged?.Invoke(true, Mathf.Abs(_verticalVelocity));
        }
        // Left the Ground
        else if (_grounded && !groundHit)
        {
            _grounded = false;
            _frameLeftGrounded = Time.time;
            GroundedChanged?.Invoke(false, 0);
        }
    }
    private void HandleJumpInput()
    {
        if (!_endedJumpEarly && !_grounded && !_input.Jump && _verticalVelocity > 0) _endedJumpEarly = true;

        if (!_jumpToConsume && !HasBufferedJump) return;

        if (_grounded || CanUseCoyote) ExecuteJump();

        _jumpToConsume = false;
    }
    private void ExecuteJump()
    {
        _endedJumpEarly = false;
        _timeJumpWasPressed = 0;
        _bufferedJumpUsable = false;
        _coyoteUsable = false;
        _verticalVelocity = _stats.JumpPower;
        Jumped?.Invoke();
    }
    private void HandleDash()
    {
        // Vector3.Distance(transform.position, _dashObject.position) < _stats.SlowdownDistanceTrigger <-- instead of raycast?

        if (_dashing)
        {
            if (Time.time >= _timeStartedDash + _stats.DashDuration || Physics.Raycast(transform.position, _dashDirection, out _dashHit, 1, _stats.GroundLayer))
            {
                _dashObject = null;
                _dashing = false;
                _dashSlowdown = true;
                _verticalVelocity = 0;
                _dashSlowdownTimer = 0;
            }
        }
        else
        {
            _dashDirection = Vector3.Lerp(_dashDirection, Vector3.zero, _dashSlowdownTimer / _stats.DashSlowdownDuration);
        }
    }
    private void ExecuteMagneticAction()
    {
        _dashObject = _magneticSphere.GetClosestObject();

        if (_dashObject == null) return;


        if (_dashObject.TryGetComponent(out LightMagneticCube magneticCubeScript))
        {
            // light object: push/pull the object
            Vector3 direction = _magneticPull ? (transform.position - _dashObject.position).normalized : (_dashObject.position - transform.position).normalized;
            magneticCubeScript.ExecuteDash(direction, _stats.DashDuration, _stats.DashSpeed);
        }
        else
        {
            // heavy object: push/pull the player, making them dash
            _dashDirection = _magneticPull ? (_dashObject.position - transform.position).normalized : (transform.position - _dashObject.position).normalized;

            _dashing = true;
            _timeStartedDash = Time.time;
        }

        _magneticActionCooldownTimer = 0;
    }
    private void HandleDirection()
    {
        if (_dashing || _dashSlowdown) return;

        float targetSpeed = _stats.MaxSpeed;

        if (_input.MoveVector == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.UseAnalogMovement ? _input.MoveVector.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _currentSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.fixedDeltaTime * _stats.Acceleration);
            _currentSpeed = Mathf.Round(_currentSpeed * 1000f) / 1000f;
        }
        else
        {
            _currentSpeed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(_input.MoveVector.x, 0.0f, _input.MoveVector.y).normalized;

        if (_input.MoveVector != Vector2.zero)
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, _stats.RotationSmoothTime);

            // rotate to face input direction relative to camera position
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        _targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;
        if (_onSlope) _targetDirection = Vector3.zero;
    }
    private void HandleGravity()
    {
        if (_grounded && _verticalVelocity <= 0f)
        {
            _verticalVelocity = _stats.GroundingForce;
        }
        else
        {
            var inAirGravity = _stats.FallAcceleration;
            if (_endedJumpEarly && _verticalVelocity > 0)
            {
                inAirGravity *= _stats.JumpEndEarlyGravityModifier;
            }
            _verticalVelocity = Mathf.MoveTowards(_verticalVelocity, -_stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
        }
    }
    private void ApplyMovement()
    {
        if (_dashing)
        {
            _controller.Move(_dashDirection * _stats.DashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            _controller.Move((_targetDirection.normalized * _currentSpeed + new Vector3(0.0f, _verticalVelocity, 0.0f) + _dashDirection * _stats.DashSpeed) * Time.fixedDeltaTime);
            /* if (_onSlope)
            {
                _controller.Move((_targetDirection.normalized * _currentSpeed + new Vector3(0.0f, _verticalVelocity, 0.0f) + _dashDirection * _stats.DashSpeed + _slopeDirection * _stats.SlopeFallPower) * Time.fixedDeltaTime);
            }
            else
            {
                _controller.Move((_targetDirection.normalized * _currentSpeed + new Vector3(0.0f, _verticalVelocity, 0.0f) + _dashDirection * _stats.DashSpeed) * Time.fixedDeltaTime);
            } */
        }
    }
}
