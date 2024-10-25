using UnityEngine;

public class AirDash : GearBase
{
    [SerializeField]
    private float _dashSpeed;

    [SerializeField]
    private float _dashCooldown;

    [SerializeField]
    private float _retainedVelocityRatio;

    [SerializeField]
    private GameObject _visuals;

    [SerializeField]
    private Animator _animator;

    private float _dashTimer;

    private void Update()
    {
        if (_dashTimer > 0f)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer < 0f)
            {
                _animator.Play("FeatherIdle");
            }
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
            if (_dashTimer <= 0f)
            {
                _animator.Play("FeatherUsed");
                Protag.MoveController.ForceUnground();

                Vector3 oldVelocity = Protag.MoveController.Velocity;

                float dot = Mathf.Max(0, Vector3.Dot(oldVelocity.normalized, direction));
                Vector3 newVelocity = direction * (_dashSpeed + _retainedVelocityRatio * dot * oldVelocity.magnitude);

                Protag.MoveController.Velocity = newVelocity;
                _dashTimer = _dashCooldown;

                AkSoundEngine.PostEvent("playAirDash", gameObject);
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