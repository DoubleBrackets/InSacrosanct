using UnityEngine;

public class AirDash : GearBase
{
    [SerializeField]
    private float _dashSpeed;

    [SerializeField]
    private float _dashCooldown;

    [SerializeField]
    private GameObject _visuals;

    [SerializeField]
    private Animator _animator;

    private float _dashTime;

    private void Update()
    {
        if (Time.time < _dashTime && Time.time + Time.deltaTime >= _dashTime)
        {
            _animator.Play("FeatherIdle");
        }
    }

    public override void OnEquip()
    {
    }

    public override void Tick()
    {
        Vector2 inputRaw = InputProvider.GetMoveInput();
        if (inputRaw == Vector2.zero)
        {
            inputRaw = Vector2.up;
        }

        Vector3 direction = Protag.FpCamera.TransformDirection(new Vector3(inputRaw.x, 0, inputRaw.y)).normalized;

        if (Input.GetMouseButtonDown(0) && !Protag.MoveController.IsSurfing)
        {
            if (Time.time >= _dashTime)
            {
                _animator.Play("FeatherUsed");
                Protag.MoveController.ForceUnground();

                Vector3 oldVelocity = Protag.MoveController.Velocity;
                Vector3 newVelocity = direction * _dashSpeed;

                Protag.MoveController.Velocity = newVelocity;
                _dashTime = Time.time + _dashCooldown;
            }
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