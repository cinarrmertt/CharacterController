using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLocomotionMap : MonoBehaviour,InputControls.IPlayerActionsActions
{
    [SerializeField]private bool holdtoSprint=true;
    
    public InputControls _inputControls { get; private set; } 
    public Vector2 _moveInput { get; private set; }
    public Vector2 _lookInput { get; private set; }
    public bool _sprintToggleOn { get; private set; }
    
    public bool _jumpPressed { get; private set; }

    
    public static PlayerLocomotionMap instance;

    private void Awake()
    {
        if (instance !=null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        _inputControls = new InputControls();
        _inputControls.Enable();
        
        _inputControls.PlayerActions.Enable();
        _inputControls.PlayerActions.SetCallbacks(this);
    }

    private void OnDisable()
    {
        _inputControls.PlayerActions.Disable();
        _inputControls.PlayerActions.RemoveCallbacks(this);
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
            _sprintToggleOn = holdtoSprint || !_sprintToggleOn;
        }
        else if (context.canceled)
        {
            _sprintToggleOn = !holdtoSprint && _sprintToggleOn;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        _jumpPressed = true;
    }
}
