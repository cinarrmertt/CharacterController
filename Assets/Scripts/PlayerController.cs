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
    }

    private void Update()
    {
        UpdateMovementState();
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

        StatsType lateralState = isSprinting ? StatsType.Sprinting : 
            isMovingLaterally || isMovementInput ? StatsType.Running : StatsType.Idling;
        
        _playerState.SetPlayerMovementState(lateralState);

    }
    void HandleLateralMovement()
    {
        bool isSprinting = _playerState.currentStat==StatsType.Sprinting;
        
        float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralSpeed = isSprinting ? sprintSpeed : runSpeed;
        
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
        newVelocity=Vector3.ClampMagnitude(newVelocity, clampLateralSpeed);
        
        _characterController.Move(newVelocity*Time.deltaTime);
    }
    bool IsMovingLaterally()
    {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x,0,_characterController.velocity.z);
        
        return lateralVelocity.magnitude > movingThreshold;
    }
    
}
