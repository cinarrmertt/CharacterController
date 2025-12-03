using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private PlayerLocomotionMap _playerLocomotionMap;
    private PlayerState _playerState;
    private CharacterController _characterController;
    public Camera _playerCamera;
    
    [Header("Movement Settings")]
    [SerializeField] private float runAcceleration = 0.25f;
    [SerializeField] private float runSpeed = 4f;
    [SerializeField]private float sprintAcceleration = 0.5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float drag = 0.1f;
    [SerializeField] private float movingThreshold = 0.01f;
    private Vector3 _moveDirection = Vector3.zero;
    private float stepOffset;
    
    [Header("Gravity Settings")]
    [SerializeField] private float gravity = 25f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float inAirAcceleration = 0.15f;
    [SerializeField] private LayerMask groundLayers;
    private float verticalVelocity = 0f;
    private float antiBump;

    [Header("Camera Settings")] 
    [SerializeField] private float lookSenseH = 0.1f;
    [SerializeField] private float lookSenseV = 0.1f;
    [SerializeField] private float lookLimitV = 80f;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    private void Awake()
    {
        _playerLocomotionMap = GetComponent<PlayerLocomotionMap>();
        _characterController=GetComponent<CharacterController>();
        _playerState=GetComponent<PlayerState>();

        antiBump = sprintSpeed;
        stepOffset = _characterController.stepOffset;
    }

    private void Update()
    {
        UpdateMovementState();
        HandleVerticalMovement();
        HandleLateralMovement();
    }

    private void LateUpdate()
    {
        _cameraRotation.x += lookSenseH * _playerLocomotionMap._lookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionMap._lookInput.y
            , -lookLimitV, lookLimitV);
        
        _playerTargetRotation.x += transform.eulerAngles.x +lookSenseH * _playerLocomotionMap._lookInput.x;
        transform.rotation = Quaternion.Euler(0, _playerTargetRotation.x, 0);

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0);
    }

    private void UpdateMovementState()
    {
        bool isMovementInput= _playerLocomotionMap._moveInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionMap._sprintToggleOn && isMovementInput;
        bool isGrounded = IsGrounded();

        StateType lateralState = isSprinting ? StateType.Sprinting : 
            isMovingLaterally || isMovementInput ? StateType.Running : StateType.Idling;
        
        _playerState.SetPlayerMovementState(lateralState);

        if (!isGrounded && _characterController.velocity.y >= 0)
        {
            _playerState.SetPlayerMovementState(StateType.Jumping);
            _characterController.stepOffset = 0;
        }
        else if(!isGrounded && _characterController.velocity.y < 0)
        {
            _playerState.SetPlayerMovementState(StateType.Falling);
            _characterController.stepOffset = 0;
        }
        else
        {
            _characterController.stepOffset = stepOffset;
        }
    }
    void HandleVerticalMovement()
    {
        bool isGrounded = _playerState.InGroundedState();

        verticalVelocity -= gravity * Time.deltaTime;
        
        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -antiBump;

        if (_playerLocomotionMap._jumpPressed && isGrounded)
        {
            verticalVelocity += antiBump + Mathf.Sqrt(jumpHeight * 3f * gravity);
        }
    }
    void HandleLateralMovement()
    {
        bool isSprinting = _playerState.currentStat==StateType.Sprinting;
        bool isGrounded = _playerState.InGroundedState();

        float lateralAcceleration =
            !isGrounded ? inAirAcceleration : isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralSpeed = !isGrounded ? sprintSpeed : isSprinting ? sprintSpeed : runSpeed;
         
        Vector3 cameraForward = new Vector3(_playerCamera.transform.forward.x, 
            0, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(_playerCamera.transform.right.x, 
            0, _playerCamera.transform.right.z).normalized;
        
        Vector3 movementDirection= cameraForward * _playerLocomotionMap._moveInput.y + 
                                 cameraRight * _playerLocomotionMap._moveInput.x;
        
        Vector3 movementDelta= movementDirection * lateralAcceleration * Time.deltaTime;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity=Vector3.ClampMagnitude(new Vector3(newVelocity.x,0,newVelocity.z), clampLateralSpeed);
        newVelocity.y += verticalVelocity;
        newVelocity = !isGrounded ? HandleSteepWalls(newVelocity) : newVelocity;
        
        _characterController.Move(newVelocity*Time.deltaTime);
    }
    
    private Vector3 HandleSteepWalls(Vector3 velocity)
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController,
            groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        if (!validAngle && verticalVelocity < 0f)
            velocity = Vector3.ProjectOnPlane(velocity, normal);

        return velocity;
    }
    
    bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x,0,_characterController.velocity.z);
        
        return lateralVelocity.magnitude > movingThreshold;
    }

    bool IsGrounded()
    {
        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

        return grounded;
    }
    private bool IsGroundedWhileGrounded()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, 
            transform.position.y - _characterController.radius, 
            transform.position.z);

        bool grounded = Physics.CheckSphere(spherePosition, _characterController.radius, 
            groundLayers, QueryTriggerInteraction.Ignore);

        return grounded;
    }

    private bool IsGroundedWhileAirborne()
    {
        Vector3 normal = CharacterControllerUtils.GetNormalWithSphereCast(_characterController, groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        print(angle);
        bool validAngle = angle <= _characterController.slopeLimit;
        
        return _characterController.isGrounded;
    }

}
