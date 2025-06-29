using UnityEngine;
using UnityEngine.InputSystem;
[DefaultExecutionOrder(-3)]
public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager instance;
    public InputControls _inputControls {  get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        _inputControls = new InputControls();
        _inputControls.Enable();
    }

    private void OnDisable()
    {
        _inputControls.Disable();
    }
}
