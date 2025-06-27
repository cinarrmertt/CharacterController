using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    private PlayerController _playerController;
    
    [SerializeField] private float locomotionBlendSpeed=0.02f;
    
    //ilk başta animasyon parametrelerinin referansını oluşturuyoruz.
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isRotationMismatchHash = Animator.StringToHash("rotationMismatch");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");


    private Vector3 currentBlendInput = Vector3.zero;
    private PlayerState _playerState;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    public void UpdateAnimationState()
    {
        //statlerin durumunu referans aldık.
        bool isIdling = _playerState.currentPlayerState == PlayerMovementState.Idling;
        bool isRunning = _playerState.currentPlayerState == PlayerMovementState.Running;
        bool isSprinting = _playerState.currentPlayerState == PlayerMovementState.Sprinting;
        bool isJumping = _playerState.currentPlayerState == PlayerMovementState.Jumping;
        bool isFalling = _playerState.currentPlayerState == PlayerMovementState.Falling;
        bool isGrounded = _playerState.InGroundedState();
        
        
        
        //kalvyeden aldığımız input girdisinin referansı.Eğer koşuyorsa inputun 1.5 çarpanı.
        Vector2 inputTarget = isSprinting ? PlayerLocomotionMap.instance._moveInput * 1.5f : isRunning ? 
            PlayerLocomotionMap.instance._moveInput * 1 : PlayerLocomotionMap.instance._moveInput*0.5f;
        
        currentBlendInput= Vector3.Slerp(currentBlendInput, inputTarget, locomotionBlendSpeed*Time.deltaTime);

        //inputun girdisi ile tanımladığımız için currentblendınputtan çektik.
        float inputMagnitude = currentBlendInput.magnitude;
        
        _animator.SetBool(isGroundedHash,isGrounded);
        _animator.SetBool(isIdlingHash,isIdling);
        _animator.SetBool(isJumpingHash,isJumping);
        _animator.SetBool(isRotatingToTargetHash,_playerController.isRotationToTarget);
        _animator.SetBool(isFallingHash,isFalling);
        _animator.SetFloat(inputXHash,currentBlendInput.x);
        _animator.SetFloat(inputYHash,currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash,inputMagnitude);
        _animator.SetFloat(isRotationMismatchHash,_playerController._rotationMismatch);
    }
}
