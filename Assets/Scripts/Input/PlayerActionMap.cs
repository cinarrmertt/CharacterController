using System;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-2)]
public class PlayerActionMap : MonoBehaviour,InputControls.IPlayerActionActions
{ 
    public bool punchToggleOn { get; private set; }
    private void OnEnable()
    {
        if (PlayerInputManager.instance?._inputControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot enable");
            return;
        }
        
        PlayerInputManager.instance._inputControls.PlayerAction.Enable();
        PlayerInputManager.instance._inputControls.PlayerAction.SetCallbacks(this);
    }

    private void OnDisable()
    {
        if (PlayerInputManager.instance?._inputControls == null)
        {
            Debug.LogError("Player controls is not initialized - cannot disable");
            return;
        }
        
        PlayerInputManager.instance._inputControls.PlayerAction.Disable();
        PlayerInputManager.instance._inputControls.PlayerAction.RemoveCallbacks(this);
    }
    public void SetAttackPressedFalse()
    {
        punchToggleOn = false;
    }

    public void OnPunch(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;

        punchToggleOn = true;
    }
}
