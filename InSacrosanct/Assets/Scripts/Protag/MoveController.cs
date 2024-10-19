using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveController : MonoBehaviour
{
    [Serializable]
    private struct MoveSettings
    {
        public float MoveSpeed;
        public float JumpHeight;
        public float Gravity;
        public float Acceleration;
        public float Friction;

        [Header("Ground")]

        public float SweepDistance;

        public LayerMask GroundMask;
        public Transform GroundSweepOrigin;

        [Header("Surf")]

        public Transform SurfRaycastCenter;

        public float SurfCastDist;
        public LayerMask SurfMask;

        public float SurfGravity;
        public float SurfInitiateAngleRange;

        public float SurfSteeringSpeed;

        [FormerlySerializedAs("SurfJumpVel")]
        public float SurfJumpHeight;

        public float SurfStickForce;
        public float SurfEjectForce;
        public float SurfEjectForceForward;
        public float SurfDebounceTime;

        public float SurfEnterSpeedRequirement;
        public float SurfMaintainSpeedRequirement;
    }

    public enum State
    {
        Default,
        NoMove
    }

    [Header("Dependencies")]

    [SerializeField]
    private CharacterController _characterController;

    [Header("Config")]

    [SerializeField]
    private MoveSettings _moveSettings;

    private bool IsGrounded => (_characterController.isGrounded || _castGround) && _forceUngroundTimer <= 0f;
    public bool IsSurfing => _isSurfing;
    public int SurfSide => _surfSide;

    private Vector3 _velocity;
    private Vector3 _groundNormal;
    private Vector3 _groundHitPoint;
    private bool _castGround;

    private bool _wasGrounded;
    private bool _didSnap;
    private Vector3 _lastTrace;

    private float _forceUngroundTimer;

    private bool _surfContact;
    private Vector3 _surfNormal;
    private bool _isSurfing;
    private bool _wasSurfing;
    private int _surfSide;

    private float _surfDebounceTimer;

    private void Awake()
    {
        DebugHUD.AddString(DebugString);
    }

    public void TickMovement(Vector2 rawInput, bool jumpInput, FpCamera fpCamera)
    {
        CheckGroundNormal();
        UpdateSurfDetect(fpCamera);

        _didSnap = false;
        _forceUngroundTimer -= Time.deltaTime;
        _surfDebounceTimer -= Time.deltaTime;

        SnapToGround();

        Vector3 moveInput = MoveInput(rawInput, fpCamera);

        // Standard Horizontal Movement
        if (IsGrounded || !_surfContact)
        {
            Vector3 hInput = Vector3.ProjectOnPlane(moveInput, _groundNormal);
            hInput.Normalize();
            Vector3 desiredHVel = hInput * _moveSettings.MoveSpeed;

            Vector3 hVelocity = Vector3.ProjectOnPlane(_velocity, _groundNormal);

            float finalAccel = Vector3.Dot(hVelocity, desiredHVel) <= 0
                ? _moveSettings.Friction
                : _moveSettings.Acceleration;

            Vector3 newHVel = Vector3.Lerp(
                hVelocity,
                desiredHVel,
                finalAccel * Time.deltaTime);

            _velocity += newHVel - hVelocity;
        }

        // Wallsurfing
        bool surfAim = Mathf.Abs(Vector3.Angle(_surfNormal, fpCamera.CameraForward) - 90f) <
                       _moveSettings.SurfInitiateAngleRange / 2f;

        _isSurfing = false;
        bool sufficientSpeed = _velocity.magnitude >
                               (_wasSurfing
                                   ? _moveSettings.SurfMaintainSpeedRequirement
                                   : _moveSettings.SurfEnterSpeedRequirement);

        if (!IsGrounded && _surfContact && surfAim && sufficientSpeed && rawInput.y > 0f && _surfDebounceTimer <= 0f)
        {
            Vector3 desiredDir = Vector3.ProjectOnPlane(fpCamera.CameraForward, _surfNormal).normalized;
            Vector3 currentDir = Vector3.ProjectOnPlane(_velocity, _surfNormal).normalized;

            Vector3 newDir = Vector3.RotateTowards(currentDir, desiredDir,
                Mathf.Deg2Rad * _moveSettings.SurfSteeringSpeed * Time.deltaTime,
                0f);

            float newSpeed = Vector3.ProjectOnPlane(_velocity, _surfNormal).magnitude;

            _velocity = newSpeed * newDir;
            float angle = Vector3.Angle(_velocity, Vector3.up);
            float gravityFactor = Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad));

            _velocity.y += _moveSettings.SurfGravity * Time.deltaTime * gravityFactor;

            _velocity += -_surfNormal * _moveSettings.SurfStickForce;

            _isSurfing = true;
        }
        else if (_wasSurfing)
        {
            _velocity += _surfNormal * _moveSettings.SurfEjectForce;
            _velocity += fpCamera.CameraForward * _moveSettings.SurfEjectForceForward;
            _velocity.y += Mathf.Sqrt(_moveSettings.SurfJumpHeight * -2f * _moveSettings.Gravity);
            _surfDebounceTimer = _moveSettings.SurfDebounceTime;
        }

        _wasSurfing = _isSurfing;

        // Gravity
        if (!IsGrounded)
        {
            if (!IsSurfing)
            {
                _velocity.y += _moveSettings.Gravity * Time.deltaTime;
            }
        }

        // Jump
        if (jumpInput)
        {
            if (IsGrounded)
            {
                _velocity.y = Mathf.Sqrt(_moveSettings.JumpHeight * -2f * _moveSettings.Gravity);
            }
            else if (IsSurfing)
            {
                _surfDebounceTimer = _moveSettings.SurfDebounceTime;
            }

            _forceUngroundTimer = 0.1f;
        }

        // Movement trace
        Vector3 current = _characterController.transform.position - Vector3.up;
        Vector3 offset = Vector3.forward * (Time.frameCount % 2) * 0.1f;

        Debug.DrawLine(current + offset, current + _groundNormal + offset,
            _didSnap ? Color.green : Color.red, 10f);
        Debug.DrawLine(_lastTrace + offset, current + offset,
            _characterController.isGrounded ? Color.green : Color.red, 10f);
        Debug.DrawLine(current + offset, current + _velocity * Time.deltaTime * 1.1f + offset,
            Color.yellow, 10f);

        _lastTrace = current;

        // Move and Update Velocity
        _wasGrounded = IsGrounded;
        _characterController.Move(_velocity * Time.deltaTime);
        _velocity = _characterController.velocity;
    }

    private void OnDestroy()
    {
        DebugHUD.RemoveString(DebugString);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 rayPos = _moveSettings.GroundSweepOrigin.position;
        Gizmos.DrawRay(rayPos, Vector3.down * _moveSettings.SweepDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(_groundHitPoint, _groundNormal);

        // draw surf capsule
        Gizmos.color = Color.blue;
        Vector3 pos = _moveSettings.SurfRaycastCenter.position;
        Gizmos.DrawLine(pos + Vector3.right * _moveSettings.SurfCastDist,
            pos - Vector3.right * _moveSettings.SurfCastDist);
    }

    private string DebugString()
    {
        return $"Velocity: {_velocity} " +
               $"\n Speed: {_velocity.magnitude}" +
               $"\n Grounded: {IsGrounded}" +
               $"\n Ground Normal: {_groundNormal}" +
               $"\n Did Snap: {_didSnap}" +
               $"\n Surf Contact: {_surfContact}" +
               $"\n Surf Aiming: {_isSurfing}" +
               $"\n Surfing: {IsSurfing}";
    }

    private void SnapToGround()
    {
        if (IsGrounded && Vector3.Dot(_velocity, _groundNormal) > 0)
        {
            _velocity = Vector3.ProjectOnPlane(_velocity, _groundNormal);
            _didSnap = true;
        }
    }

    private Vector3 MoveInput(Vector2 rawInput, FpCamera fpCamera)
    {
        var moveInput = new Vector3(rawInput.x, 0, rawInput.y);
        moveInput.Normalize();
        moveInput = fpCamera.TransformDirection(moveInput);
        return moveInput;
    }

    private void CheckGroundNormal()
    {
        Vector3 start = _moveSettings.GroundSweepOrigin.position;
        var ray = new Ray(start, Vector3.down);

        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, _moveSettings.SweepDistance, _moveSettings.GroundMask);

        _groundNormal = hit ? hitInfo.normal : Vector3.up;
        _groundHitPoint = hit ? hitInfo.point : start + Vector3.down * _moveSettings.SweepDistance;
        _castGround = hit;
    }

    private void UpdateSurfDetect(FpCamera fpCamera)
    {
        Vector3 camRight = fpCamera.CameraRight;
        Vector3 pos = _moveSettings.SurfRaycastCenter.position;
        bool hitRight = Physics.Raycast(
            pos,
            camRight * _moveSettings.SurfCastDist,
            out RaycastHit surfHitRight,
            _moveSettings.SurfCastDist,
            _moveSettings.SurfMask);

        bool hitLeft = Physics.Raycast(
            pos,
            -camRight * _moveSettings.SurfCastDist,
            out RaycastHit surfHitLeft,
            _moveSettings.SurfCastDist,
            _moveSettings.SurfMask);

        Vector3 surfNormal = Vector3.zero;
        if (hitLeft || hitRight)
        {
            surfNormal = hitRight ? surfHitRight.normal : surfHitLeft.normal;
            _surfSide = hitRight ? 1 : -1;
        }

        bool overHang = Vector3.Dot(surfNormal, Vector3.up) < 0f;
        if (!overHang)
        {
            _surfNormal = surfNormal;
        }


        _surfContact = hitRight ^ hitLeft && !overHang;
    }

    public float GetCurrentSpeedFactor()
    {
        return _velocity.magnitude / (_moveSettings.MoveSpeed * 2f);
    }
}