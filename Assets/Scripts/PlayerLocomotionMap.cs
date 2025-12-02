using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionMap : MonoBehaviour,PlayerControls.IPlayerMapActions
{
    [SerializeField] private bool holdToSprint = true;
    public PlayerControls _playerControls { get; private set; }
    public Vector2 _moveInput { get; private set; }
    public Vector2 _lookInput { get; private set; }
    public bool _sprintToggleOn { get; private set; }
    public bool _jumpPressed { get; private set; }

    private void OnEnable()
    {
        _playerControls=new PlayerControls();
        _playerControls.Enable();
        
        _playerControls.PlayerMap.Enable();
        _playerControls.PlayerMap.SetCallbacks(this);
    }

    private void OnDisable()
    {
        _playerControls.PlayerMap.Disable();
        _playerControls.PlayerMap.RemoveCallbacks(this);
    }

    private void LateUpdate()
    {
        _jumpPressed = false;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput=context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput=context.ReadValue<Vector2>();
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
}
