using System;
using UnityEngine;

public class GrappleHook : GearBase
{
    [Serializable]
    private struct GrappleSettings
    {
        public float GrappleRange;
        public float GrappleCooldown;
        public float PullForce;
        public float MinDistance;
        public LayerMask GrappleMask;

        public Transform GrappleOrigin;

        [Header("Visuals")]

        public LineRenderer GrappleLine;
    }

    [SerializeField]
    private GrappleSettings _settings;

    [SerializeField]
    public Animator _animator;

    [SerializeField]
    private GameObject _visuals;

    private Vector3 _grappleTarget;
    private Vector3 _grappleHoveringTarget;

    private bool _grappling;

    private void GrappleTick(Vector3 forward, bool grapplePressed, bool grappleReleased)
    {
        bool didHit = Physics.Raycast(
            _settings.GrappleOrigin.position,
            forward,
            out RaycastHit hit,
            _settings.GrappleRange,
            _settings.GrappleMask);
        if (didHit)
        {
            _grappleHoveringTarget = hit.point;
        }

        float distance = Vector3.Distance(_settings.GrappleOrigin.position, _grappleTarget);
        bool inRange = distance < _settings.GrappleRange && distance > _settings.MinDistance;

        if (!_grappling && didHit && grapplePressed && inRange)
        {
            _animator.Play("GrappleUsing");
            _settings.GrappleLine.enabled = true;
            _settings.GrappleLine.positionCount = 2;
            _grappling = true;
            _grappleTarget = hit.point;
        }

        if (_grappling)
        {
            Protag.MoveController.ForceUnground();
            Vector3 grappleOrigin = _settings.GrappleOrigin.position;
            Vector3 direction = (_grappleTarget - grappleOrigin).normalized;

            Vector3 vel = Protag.MoveController.Velocity;

            Vector3 newVel = Vector3.ProjectOnPlane(vel, direction);
            newVel = newVel.normalized * vel.magnitude;

            if (Vector3.Dot(vel, direction) < 0)
            {
                Protag.MoveController.Velocity = newVel;
            }

            _settings.GrappleLine.SetPosition(0, grappleOrigin);
            _settings.GrappleLine.SetPosition(1, _grappleTarget);

            if (grappleReleased || distance < _settings.MinDistance)
            {
                StopGrapple();
            }
        }
    }

    public override void OnEquip()
    {
    }

    public override void Tick()
    {
        Vector3 forward = Protag.FpCamera.TransformDirection(Vector3.forward);
        bool grapplePressed = Input.GetMouseButtonDown(0);
        bool grappleReleased = Input.GetMouseButtonUp(0);

        GrappleTick(forward, grapplePressed, grappleReleased);
    }

    public override void OnUnequip()
    {
        StopGrapple();
    }

    public override void Show(bool visible)
    {
        _visuals.SetActive(visible);
    }

    public override bool IsUsing()
    {
        return _grappling;
    }

    private void StopGrapple()
    {
        _animator.Play("GrappleIdle");
        _grappling = false;
        _settings.GrappleLine.enabled = false;
    }
}