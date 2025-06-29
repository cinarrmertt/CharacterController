using System;
using System.Linq;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator _animator;
    private PlayerLocomotionMap _playerLocomotionMap;
    private PlayerActionMap _playerActionMap;
    private PlayerController _playerController;
    private PlayerState _playerState;
    [Header("Locomotion Animations")]
    //ilk başta animasyon parametrelerinin referansını oluşturuyoruz.
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isCrouchingHash = Animator.StringToHash("isCrouching");
    private static int isRotationMismatchHash = Animator.StringToHash("rotationMismatch");
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    
    [Header("Action Animations")]
    private static int isAttackingHash = Animator.StringToHash("isAttacking");
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    private int[] actionHashes;

    [Header("blabla")]
    [SerializeField] private float locomotionBlendSpeed=0.02f;
    private Vector3 currentBlendInput = Vector3.zero;
    private void Awake()
    {
        _playerLocomotionMap = GetComponent<PlayerLocomotionMap>();
        _playerActionMap = GetComponent<PlayerActionMap>();
        _playerController = GetComponent<PlayerController>();
        _playerState = GetComponent<PlayerState>();

        actionHashes = new[] { isAttackingHash };
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
        bool isCrouching = _playerState.currentPlayerState == PlayerMovementState.Crouching;
        bool isGrounded = _playerState.InGroundedState();
        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));
        
        
        
        //kalvyeden aldığımız input girdisinin referansı.Eğer koşuyorsa inputun 1.5 çarpanı.
        Vector2 inputTarget = isSprinting ? _playerLocomotionMap._moveInput * 1.5f : isRunning ? 
            _playerLocomotionMap._moveInput * 1 : _playerLocomotionMap._moveInput*0.5f;
        
        currentBlendInput= Vector3.Slerp(currentBlendInput, inputTarget, locomotionBlendSpeed*Time.deltaTime);

        //inputun girdisi ile tanımladığımız için currentblendınputtan çektik.
        float inputMagnitude = currentBlendInput.magnitude;
        
        _animator.SetBool(isGroundedHash,isGrounded);
        _animator.SetBool(isIdlingHash,isIdling);
        _animator.SetBool(isJumpingHash,isJumping);
        _animator.SetBool(isCrouchingHash,isCrouching);
        _animator.SetBool(isRotatingToTargetHash,_playerController.isRotationToTarget);
        _animator.SetBool(isFallingHash,isFalling);
        _animator.SetBool(isAttackingHash,_playerActionMap.punchToggleOn);
        _animator.SetBool(isPlayingActionHash,isPlayingAction);
        
        _animator.SetFloat(inputXHash,currentBlendInput.x);
        _animator.SetFloat(inputYHash,currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash,inputMagnitude);
        _animator.SetFloat(isRotationMismatchHash,_playerController._rotationMismatch);
    }
}
