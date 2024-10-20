using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class Protag : MonoBehaviour, ILocatableService
{
    public enum ProtagState
    {
        Default,
        NoMove,
        Dead,
        UsingGear
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

    [SerializeField]
    private GearRegistry _gearRegistry;

    [SerializeField]
    private GearHandler _gearHandler;

    public MoveController MoveController => _moveController;

    public Vector3 Pos => _moveController.Position;
    public FpCamera FpCamera => _fpCamera;

    public FPCameraEffects FPCameraEffects => _fpCameraEffects;

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

    private void Start()
    {
        int currentLevel = _coreService.Instance.CurrentLevel;
        Debug.Log(currentLevel);
        List<GearBase> equippedGears = _gearRegistry.GetGearEntries(currentLevel).ToList();

        _gearHandler.Initialize(this, _inputProvider.Instance, equippedGears);
    }

    private void Update()
    {
        Vector2 rawInput = _inputProvider.Instance.GetMoveInput();
        Vector2 lookInput = _inputProvider.Instance.GetLookInput();
        bool jumpPressed = _inputProvider.Instance.GetJumpInput();

        if (_state == ProtagState.Default)
        {
            _gearHandler.Tick();

            if (_gearHandler.IsUsing)
            {
                _state = ProtagState.UsingGear;
                return;
            }

            _moveController.TickMovement(rawInput, jumpPressed, _fpCamera);
        }
        else
        {
            _moveController.TickMovement(Vector2.zero, false, _fpCamera);
        }

        if (_state == ProtagState.UsingGear)
        {
            _gearHandler.Tick();
            if (!_gearHandler.IsUsing)
            {
                _state = ProtagState.Default;
            }
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
        _gearHandler.UnequipGear();
    }

    public void Kill(TimelineAsset deathTimeline)
    {
        if (_state == ProtagState.Dead)
        {
            return;
        }

        _state = ProtagState.Dead;
        _coreService.Instance.Death(deathTimeline);
        _gearHandler.UnequipGear();

        AkSoundEngine.PostEvent("playDeath", gameObject);
        AudioManager.Instance.StopMusic();
    }
}