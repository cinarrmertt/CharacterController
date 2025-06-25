using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float runSpeed;
    [SerializeField] private float runAcceleration;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float sprintAcceleration;
    [SerializeField] private float drag=0.1f;
    [SerializeField] private float movingThreshold=0.01f;
    [Header("Camera")]
    [SerializeField] private float lookSenseH=0.1f;
    [SerializeField] private float lookSenseV=0.1f;
    [SerializeField] private float lookLimitMinV=30f;
    [SerializeField] private float lookLimitMaxV=89f;
    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    [Header("References")]
    private CharacterController _characterController;
    private PlayerState _playerState;
    private Camera playerCam;
    

    

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerState = GetComponent<PlayerState>();
        playerCam = Camera.main;
    }

    private void Update()
    {
        UpdateMovementState();
       HandleLateralMovement();
    }

    private void UpdateMovementState()
    {
        //input girdisi olup olmadığını kontrol ediyoruz
        bool isMovementInput = PlayerLocomotionMap.instance._moveInput != Vector2.zero;
        //aşağıda fonksiyon var
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = PlayerLocomotionMap.instance._sprintToggleOn && isMovingLaterally;
        //eğer hareket varsa durum değişiyor.
        PlayerMovementState lateralState = isSprinting
            ? PlayerMovementState.Sprinting:
            isMovingLaterally || isMovementInput 
                ? PlayerMovementState.Running
            : PlayerMovementState.Idling;
        _playerState.SetPlayerMovementState(lateralState);
    }

    private void HandleLateralMovement()
    {
        //isSiprinting durumumu referans aldık.
        bool isSprinting = _playerState.currentPlayerState == PlayerMovementState.Sprinting;
        //yanal ivmelenme durumu 
        float lateralAcceleration = isSprinting ? sprintAcceleration : runAcceleration;
        //yanal hız durumu
        float clampLateralMagnitude = isSprinting ? sprintSpeed : runSpeed;
        
        Vector3 cameraForward = new Vector3(playerCam.transform.forward.x, 0, playerCam.transform.forward.z).normalized;
        Vector3 cameraRight = new Vector3(playerCam.transform.right.x, 0, playerCam.transform.right.z).normalized;

        Vector3 direction = cameraRight * PlayerLocomotionMap.instance._moveInput.x +
                            cameraForward * PlayerLocomotionMap.instance._moveInput.y;

        Vector3 movementDelta = direction * lateralAcceleration;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        Vector3 currentDrag = newVelocity.normalized * drag * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > drag * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(newVelocity, clampLateralMagnitude);
        
        _characterController.Move(newVelocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        // Mouse/joystick inputları ile kamera rotasyonu güncelle
        //y inputun tersini aldık.
        _cameraRotation.x += lookSenseH * PlayerLocomotionMap.instance._lookInput.x;
        _cameraRotation.y -= lookSenseV * PlayerLocomotionMap.instance._lookInput.y;
        //rotasyonu kısıtladık.
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y, lookLimitMinV, lookLimitMaxV);

        // Karakteri yatay eksende döndür (kameranın yatay açısına göre)
        transform.rotation = Quaternion.Euler(0f, _cameraRotation.x, 0f);

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
        
    }

    private bool IsMovingLaterally()
    {
        //hareket hızının belli bir değerin üstünde olup olmadığına bakıyoruz.Hareketin olup olmadığı ve hızını ölçer.
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0, _characterController.velocity.z);
        return lateralVelocity.magnitude > movingThreshold;
    }
}
