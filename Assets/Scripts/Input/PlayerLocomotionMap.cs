using System;
using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-2)]
public class PlayerLocomotionMap : MonoBehaviour,InputControls.IPlayerLocomotionActions
{
    [SerializeField]private bool holdToSprint=true;
    public Vector2 _moveInput { get; private set; }
    public Vector2 _lookInput { get; private set; }
    public bool _sprintToggleOn { get; private set; }
    public bool _jumpPressed { get; private set; }
    public bool _walkToggleOn { get; private set; }
    public bool _crouchToggleOn { get; private set; }


    private void OnEnable()
    {
        if (PlayerInputManager.instance?._inputControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }
        
        PlayerInputManager.instance._inputControls.PlayerLocomotion.Enable();
        PlayerInputManager.instance._inputControls.PlayerLocomotion.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance?._inputControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }
        
        PlayerInputManager.instance._inputControls.PlayerLocomotion.Disable();
        PlayerInputManager.instance._inputControls.PlayerLocomotion.RemoveCallbacks(this);
    }

    private void LateUpdate()
    {
        _jumpPressed = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _sprintToggleOn = holdToSprint || !_sprintToggleOn;
        }
        else if (context.canceled)
        {
            _sprintToggleOn = !holdToSprint && _sprintToggleOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        _jumpPressed = true;
    }

    public void OnToggleWalk(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;
        
        _walkToggleOn = !_walkToggleOn;
    }

    public void OnToggleCrouch(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        _crouchToggleOn = !_crouchToggleOn;
    }
}
