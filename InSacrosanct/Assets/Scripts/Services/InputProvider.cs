using UnityEngine;
using UnityEngine.InputSystem;

public class InputProvider : MonoBehaviour, InputSystem_Actions.IPlayerActions, ILocatableService
{
    [SerializeField]
    private ServiceLocator _serviceLocator;

    private Vector2 _lookInput;
    private Vector2 _moveInput;

    private InputSystem_Actions _inputSystem;

    private bool _jumpPressed;

    private void Awake()
    {
        if (_serviceLocator.Has<InputProvider>())
        {
            return;
        }

        _serviceLocator.Register(this);
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Player.SetCallbacks(this);
        _inputSystem.Enable();
    }

    private void LateUpdate()
    {
        _jumpPressed = false;
    }

    private void OnDestroy()
    {
        _serviceLocator.Deregister(this);
        _inputSystem?.Dispose();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _jumpPressed = context.ReadValueAsButton();
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }

    public Vector2 GetLookInput()
    {
        return _lookInput;
    }

    public Vector2 GetMoveInput()
    {
        return _moveInput;
    }

    public bool GetJumpInput()
    {
        return _jumpPressed;
    }
}