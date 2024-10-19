using UnityEngine;
using UnityEngine.Timeline;

public class Protag : MonoBehaviour, ILocatableService
{
    public enum ProtagState
    {
        Default,
        NoMove,
        Dead
    }

    [Header("Dependencies")]

    [SerializeField]
    private ServiceLocator _serviceLocator;

    [SerializeField]
    private MoveController _moveController;

    [SerializeField]
    private FPCameraEffects _fpCameraEffects;

    [SerializeField]
    private FpCamera _fpCamera;

    private LocatedService<InputProvider> _inputProvider;
    private LocatedService<CoreService> _coreService;

    private ProtagState _state = ProtagState.Default;

    private void Awake()
    {
        if (_serviceLocator.Has<Protag>())
        {
            Debug.LogError($"Service of type {typeof(Protag)} already registered.");
            return;
        }

        _serviceLocator.Register(this);

        _inputProvider = new LocatedService<InputProvider>(_serviceLocator);
        _coreService = new LocatedService<CoreService>(_serviceLocator);
    }

    private void Update()
    {
        Vector2 rawInput = _inputProvider.Instance.GetMoveInput();
        Vector2 lookInput = _inputProvider.Instance.GetLookInput();
        bool jumpPressed = _inputProvider.Instance.GetJumpInput();

        if (_state == ProtagState.Default)
        {
            _moveController.TickMovement(rawInput, jumpPressed, _fpCamera);
        }
        else
        {
            _moveController.TickMovement(Vector2.zero, false, _fpCamera);
        }

        if (_moveController.IsSurfing)
        {
            _fpCameraEffects.SetDutchTilt(_moveController.SurfSide);
        }
        else
        {
            _fpCameraEffects.SetDutchTilt(0);
        }

        _fpCameraEffects.SetWallRideImpulse(_moveController.IsSurfing);
        _fpCameraEffects.SetFOV(_moveController.GetCurrentSpeedFactor());
        _fpCameraEffects.Tick();

        _fpCamera.TickCamera(lookInput);
    }

    private void OnDestroy()
    {
        _serviceLocator.Deregister(this);
    }

    public void SetToCinematicState(bool setState)
    {
        _state = setState ? ProtagState.NoMove : ProtagState.Default;
    }

    public void Kill(TimelineAsset deathTimeline)
    {
        _state = ProtagState.Dead;
        _coreService.Instance.Death(deathTimeline);
    }
}