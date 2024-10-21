using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Sword : GearBase
{
    [SerializeField]
    private float _sliceCooldown;

    [SerializeField]
    private int _maxAmmo;

    [SerializeField]
    public float _staticCooldown;

    [Header("Combat")]

    [SerializeField]
    private float _recoilVelocity;

    [SerializeField]
    private float _distance;

    [SerializeField]
    private LayerMask _targetMask;

    [Header("Visuals")]

    [SerializeField]
    private GameObject _visuals;

    [SerializeField]
    private GameObject _swordProp;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private GameObject _swordAmmoPrefab;

    [SerializeField]
    private Transform _swordAmmoParent;

    [SerializeField]
    private float _ammoItemRadius;

    public UnityEvent OnFire;

    private readonly float _animationDuration = 0.6f;

    private readonly List<GameObject> _swordItems = new();

    private float _sliceCooldownTimer;

    private IKillable _target;

    private RaycastHit _hit;

    private int _ammo;

    private float _staticCooldownTimer;

    private void Awake()
    {
        // instantiate ammo items
        float angleIncrement = 2 * Mathf.PI / _maxAmmo;
        for (var i = 0; i < _maxAmmo; i++)
        {
            GameObject swordItem = Instantiate(_swordAmmoPrefab, _swordAmmoParent);
            swordItem.transform.localPosition = _ammoItemRadius *
                                                new Vector3(
                                                    Mathf.Cos(i * angleIncrement),
                                                    0,
                                                    Mathf.Sin(i * angleIncrement));
            _swordItems.Add(swordItem);
        }
    }

    private void Update()
    {
        if (_ammo < _maxAmmo)
        {
            _sliceCooldownTimer -= Time.deltaTime;
            if (_sliceCooldownTimer < 0f)
            {
                _ammo++;
                _sliceCooldownTimer += _sliceCooldown;
                UpdateAmmoVisuals();
            }
        }

        if (_staticCooldownTimer > 0f)
        {
            _staticCooldownTimer -= Time.deltaTime;
        }
    }

    public override void OnEquip()
    {
    }

    public override void Tick()
    {
        Vector3 looking = Protag.FpCamera.CameraForward;

        if (_ammo > 0 && _staticCooldownTimer <= 0f)
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

            bool validTarget = _target is { Killable: true };

            if (_staticCooldownTimer <= 0f)
            {
                _animator.Play(validTarget ? "SwordHovering" : "SwordIdle");
            }

            if (Input.GetMouseButtonDown(0) && validTarget)
            {
                PerformSlice(looking).Forget();
                _ammo--;
                _staticCooldownTimer = _staticCooldown;
                UpdateAmmoVisuals();
                OnFire.Invoke();
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
    }

    private void UpdateAmmoVisuals()
    {
        for (var i = 0; i < _maxAmmo; i++)
        {
            _swordItems[i].SetActive(i < _ammo);
        }
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