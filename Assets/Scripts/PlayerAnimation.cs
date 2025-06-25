using System;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed=0.02f;
    
    //ilk başta animasyon parametrelerinin referansını oluşturuyoruz.
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");

    private Vector3 currentBlendInput = Vector3.zero;
    private PlayerState _playerState;

    private void Awake()
    {
        _playerState = GetComponent<PlayerState>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    public void UpdateAnimationState()
    {
        //isSprinting durumunu referans aldık.
        bool isSprinting = _playerState.currentPlayerState == PlayerMovementState.Sprinting;
        
        //kalvyeden aldığımız input girdisinin referansı.Eğer koşuyorsa inputun 1.5 çarpanı.
        Vector2 inputTarget = isSprinting ? PlayerLocomotionMap.instance._moveInput * 1.5f : PlayerLocomotionMap.instance._moveInput;
        
        currentBlendInput= Vector3.Slerp(currentBlendInput, inputTarget, locomotionBlendSpeed*Time.deltaTime);

        //inputun girdisi ile tanımladığımız için currentblendınputtan çektik.
        float inputMagnitude = currentBlendInput.magnitude;
        
        _animator.SetFloat(inputXHash,currentBlendInput.x);
        _animator.SetFloat(inputYHash,currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash,inputMagnitude);
    }
}
