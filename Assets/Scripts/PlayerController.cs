using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float crouchAcceleration;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float walkAcceleration;
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sprintAcceleration;
    [SerializeField] private float drag=0.1f;
    [SerializeField] private float movingThreshold=0.01f;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float gravity;
    [SerializeField] private float crouchHeight = 1.2f;
    [SerializeField] private Vector3 crouchCenter = new Vector3(0, 0.595f, 0);

    private float verticalVelocity;
    private float rotatingToTargetTimer = 0;
    private bool _isRotatingClockwise = false;
    private float standHeight;
    private Vector3 standCenter;
    
    [Header("Animations")] 
    public float playerModelRotationSpeed = 10;
    public float rotateToTargetTime = 0.25f;
    
    [Header("Camera")]
    [SerializeField] private float lookSenseH=0.1f;
    [SerializeField] private float lookSenseV=0.1f;
    [SerializeField] private float lookLimitMinV=30f;
    [SerializeField] private float lookLimitMaxV=89f;
    
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;

    [Header("Components")]
    private CharacterController _characterController;
    private PlayerLocomotionMap _playerLocomotionMap;
    private PlayerState _playerState;
    private Camera playerCam;
    
    public float _rotationMismatch { get; private set; } = 0;
    public bool isRotationToTarget { get; private set; } = false;
    

    

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerLocomotionMap = GetComponent<PlayerLocomotionMap>();
        _playerState = GetComponent<PlayerState>();
        playerCam = Camera.main;

        standHeight = _characterController.height;
        standCenter = _characterController.center;
    }

    private void Update()
    {
        UpdateMovementState(); 
        HandleGravity(); 
        HandleLateralMovement(); 
        UpdateControllerCollider();
    }

    private void UpdateMovementState()
    {
        bool canRun = CanRun();
        //input girdisi olup olmadığını kontrol ediyoruz
        bool isMovementInput = _playerLocomotionMap._moveInput != Vector2.zero;
        //aşağıda fonksiyon var
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionMap._sprintToggleOn && isMovingLaterally;
        bool isWalking = (isMovingLaterally && !canRun) || _playerLocomotionMap._walkToggleOn;
        bool isCrouching = _playerLocomotionMap._crouchToggleOn;
        bool isGrounded = IsGrounded();
        
        //eğer hareket varsa durum değişiyor.
        PlayerMovementState lateralState = isCrouching 
            ? PlayerMovementState.Crouching:
            isWalking
            ? PlayerMovementState.Walking:
            isSprinting
            ? PlayerMovementState.Sprinting:
            isMovingLaterally || isMovementInput 
                ? PlayerMovementState.Running : 
                PlayerMovementState.Idling;
        
        _playerState.SetPlayerMovementState(lateralState);

        if (!isGrounded&&_characterController.velocity.y > 0)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Jumping);
        }
        else if (!isGrounded&&_characterController.velocity.y < 0)
        {
            _playerState.SetPlayerMovementState(PlayerMovementState.Falling);
        }
    }
    private void HandleGravity()
    {
        bool isGrounded = _playerState.InGroundedState();

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = 0;

        verticalVelocity -= gravity * Time.deltaTime;

        if (_playerLocomotionMap._jumpPressed && isGrounded)
        {
            verticalVelocity += Mathf.Sqrt(gravity * 3 * jumpSpeed);
        }
    }
    private void HandleLateralMovement()
    {
        //isSprinting durumumu referans aldık.
        bool isSprinting = _playerState.currentPlayerState == PlayerMovementState.Sprinting;
        //isGrounded referansı
        bool isGrounded = _playerState.InGroundedState();
        
        bool isWalking = _playerState.currentPlayerState == PlayerMovementState.Walking;

        bool isCrouching = _playerState.currentPlayerState == PlayerMovementState.Crouching;
        //yanal ivmelenme durumu 
        float lateralAcceleration = isCrouching ? crouchAcceleration :
        isWalking ? walkAcceleration : 
            isSprinting ? sprintAcceleration : runAcceleration;
        //yanal hız durumu
        float clampLateralMagnitude = isCrouching ? crouchSpeed :
            isWalking ? walkSpeed :
            isSprinting ? sprintSpeed : runSpeed;
        
        Vector3 cameraForward = new Vector3(playerCam.transform.forward.x, 0, playerCam.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(playerCam.transform.right.x, 0, playerCam.transform.right.z).normalized;

        Vector3 direction = cameraRight * _playerLocomotionMap._moveInput.x +
                            cameraForward * _playerLocomotionMap._moveInput.y;

        Vector3 movementDelta = direction * lateralAcceleration;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
        //yeni hızın ysine dikey hızımızı ekliyoruz.
        newVelocity.y += verticalVelocity;
        
        _characterController.Move(newVelocity * Time.deltaTime);
    }
    private void LateUpdate()
    {
        UpdateCameraRotation();
    }

    private void UpdateCameraRotation()
    {
        // Mouse/joystick inputları ile kamera rotasyonu güncelle
        //y inputun tersini aldık.
        _cameraRotation.x += lookSenseH * _playerLocomotionMap._lookInput.x;
        _cameraRotation.y -= lookSenseV * _playerLocomotionMap._lookInput.y;
        //rotasyonu kısıtladık.
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, lookLimitMinV, lookLimitMaxV);
        
        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionMap._lookInput.x;

        // Karakteri yatay eksende döndür (kameranın yatay açısına göre)
        //karakterin rotationMismatchi 90 dan ve hedef dönüş zamanlayıcısı 0 dan büyükse karakter dönsün.
        float rotationTolerance = 90f;
        bool isIdling = _playerState.currentPlayerState == PlayerMovementState.Idling;
        
        if (!isIdling)
        {
            RotatePlayerToTarget();
        }
        // If rotation mismatch not within tolerance, or rotate to target is active, ROTATE
        else if (Mathf.Abs(_rotationMismatch) > rotationTolerance || isRotationToTarget)
        {
            UpdateIdleRotation(rotationTolerance);
        }
        if (!isIdling || Mathf.Abs(_rotationMismatch) > rotationTolerance || rotatingToTargetTimer > 0)
        {
            RotatePlayerToTarget();
            
            if (Mathf.Abs(_rotationMismatch)>rotationTolerance)
            {
                rotatingToTargetTimer = rotateToTargetTime;
            }

            rotatingToTargetTimer -= Time.deltaTime;
        }

        // Kamera pozisyonunu karakterin etrafında ayarla
        float distanceFromTarget = 4f;   // Kamera karakterden ne kadar uzak olsun
        float heightOffset = 2f;         // Kamera karakterin ne kadar üstünde dursun

        // Kamera offsetini hesapla
        Quaternion rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);
        Vector3 cameraOffset = rotation * new Vector3(0f, 0f, -distanceFromTarget) + Vector3.up * heightOffset;

        // Kamera pozisyon ve rotasyonu
        //karakterin pozisyonunu camera offset ile toplayıp kameranın pozisyonunu çıkarıyoruz.
        Vector3 targetPosition = transform.position + cameraOffset;
        playerCam.transform.position = targetPosition;

        // Kamera karaktere baksın
        playerCam.transform.LookAt(transform.position + Vector3.up * heightOffset);
        
        Vector3 camForwardProjectedXZ = new Vector3(playerCam.transform.forward.x, 0f, playerCam.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        _rotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
    }
    private void UpdateIdleRotation(float rotationTolerance)
    {
        // Initiate new rotation direction
        if (Mathf.Abs(_rotationMismatch) > rotationTolerance)
        {
            rotatingToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = _rotationMismatch > rotationTolerance;
        }
        rotatingToTargetTimer -= Time.deltaTime;

        // Rotate player
        if (_isRotatingClockwise && _rotationMismatch > 0f ||
            !_isRotatingClockwise && _rotationMismatch < 0f)
        {
            RotatePlayerToTarget();
        }
    }
    private void RotatePlayerToTarget()
    {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }

    private bool IsMovingLaterally()
    {
        //hareket hızının belli bir değerin üstünde olup olmadığına bakıyoruz.Hareketin olup olmadığı ve hızını ölçer.
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);
        return lateralVelocity.magnitude > movingThreshold;
    }

    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }

    private bool CanRun()
    {
        return _playerLocomotionMap._moveInput.y >= Mathf.Abs(_playerLocomotionMap._moveInput.x);
    }

    private void UpdateControllerCollider()
    {
        Vector3 targetCenter = standCenter;
        float targetHeight = standHeight;

        if (_playerLocomotionMap._crouchToggleOn)
        {
            targetCenter = crouchCenter;
            targetHeight = crouchHeight;
        }

        _characterController.height =
            Mathf.Lerp(_characterController.height, targetHeight, crouchSpeed * Time.deltaTime);
        _characterController.center =
            Vector3.Lerp(_characterController.center, targetCenter, crouchSpeed * Time.deltaTime);
    }
}
