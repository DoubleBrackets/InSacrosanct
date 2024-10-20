using Cysharp.Threading.Tasks;
using UnityEngine;

public class Sword : GearBase
{
    private enum SwordState
    {
        Idle,
        Using,
        Cooldown
    }

    [SerializeField]
    private float _sliceCooldown;

    [SerializeField]
    private float _recoilVelocity;

    [SerializeField]
    private GameObject _swordProp;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private LayerMask _targetMask;

    [SerializeField]
    private GameObject _visuals;

    [SerializeField]
    private Animator _animator;

    private readonly float _animationDuration = 0.6f;

    private SwordState _state = SwordState.Idle;

    private float _sliceCooldownTimer;

    private IKillable _target;

    private RaycastHit _hit;

    private void Update()
    {
        if (_state == SwordState.Cooldown)
        {
            if (_sliceCooldownTimer > 0f)
            {
                _sliceCooldownTimer -= Time.deltaTime;
                if (_sliceCooldownTimer < 0f)
                {
                    _animator.Play("SwordIdle");
                    _state = SwordState.Idle;
                }
            }
        }
    }

    public override void OnEquip()
    {
    }

    public override void Tick()
    {
        Vector3 looking = Protag.FpCamera.CameraForward;

        if (_state == SwordState.Idle)
        {
            // Raycast
            if (Physics.Raycast(Protag.FpCamera.Position, looking, out RaycastHit hit, _distance, _targetMask))
            {
                var target = hit.collider.GetComponentInParent<IKillable>();
                if (target != null)
                {
                    _target = target;
                    _hit = hit;
                }
            }
            else
            {
                _target = null;
            }

            bool validTarget = _target != null && _target.Killable;

            if (validTarget)
            {
                _animator.Play("SwordHovering");
            }
            else
            {
                _animator.Play("SwordIdle");
            }

            if (Input.GetMouseButtonDown(0) && validTarget)
            {
                if (_sliceCooldownTimer <= 0f)
                {
                    PerformSlice(looking).Forget();
                    _state = SwordState.Using;
                }
            }
        }
    }

    private async UniTaskVoid PerformSlice(Vector3 looking)
    {
        _target.Kill();

        // Visuals
        _animator.Play("SwordUsing");

        Vector3 dir = _target.AnchorPoint.position - _hit.point;
        GameObject swordProp = Instantiate(_swordProp, _hit.point, Quaternion.LookRotation(dir));
        swordProp.transform.parent = _target.AnchorPoint;

        // Knockback
        Protag.MoveController.ForceUnground();
        Vector3 vel = Protag.MoveController.Velocity;

        vel = Vector3.ProjectOnPlane(vel, -looking);
        vel += -looking * _recoilVelocity;

        Protag.MoveController.Velocity = vel;
        _sliceCooldownTimer = _sliceCooldown;

        await UniTask.Delay((int)(_animationDuration * 1000));

        _animator.Play("SwordCooldown");

        _state = SwordState.Cooldown;
    }

    public override void OnUnequip()
    {
    }

    public override void Show(bool visible)
    {
        _visuals.SetActive(visible);
    }

    public override bool IsUsing()
    {
        return false;
    }
}