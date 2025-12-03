using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 4f;

    private PlayerLocomotionMap _playerLocomotionInput;
    private PlayerState _playerState;

    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isJumpingHash = Animator.StringToHash("isJumping");
    private static int isFallingHash = Animator.StringToHash("isFalling");

    private Vector3 _currentBlendInput = Vector3.zero;

    private void Awake()
    {
        _playerLocomotionInput = GetComponent<PlayerLocomotionMap>();
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void UpdateAnimationState()
    {
        bool isIdling = _playerState.currentStat == StateType.Idling;
        bool isRunning = _playerState.currentStat == StateType.Running;
        bool isSprinting = _playerState.currentStat == StateType.Sprinting;
        bool isJumping = _playerState.currentStat == StateType.Jumping;
        bool isFalling = _playerState.currentStat == StateType.Falling;
        bool isGrounded = _playerState.InGroundedState();

        Vector2 inputTarget = isSprinting ? _playerLocomotionInput._moveInput * 1.5f : 
            _playerLocomotionInput._moveInput;
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        
        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isFallingHash, isFalling);
    }
}
