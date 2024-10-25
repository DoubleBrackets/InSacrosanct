using System;
using UnityEngine;
using UnityEngine.Serialization;

public class MoveController : MonoBehaviour
{
    public enum State
    {
        Default,
        NoMove
    }

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

        public float MaxGroundAngle;

        [Header("Surf")]

        public Transform SurfRaycastCenter;

        public float SurfCastDist;
        public LayerMask SurfMask;

        [Tooltip("Max angle of surfing surface normal from horizontal")]
        public float SurfSurfaceAngleRange;

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

    [Header("Dependencies")]

    [SerializeField]
    private CharacterController _characterController;

    [Header("Config")]

    [SerializeField]
    private MoveSettings _moveSettings;

    [Header("Audio Settings")]

    public float footstepDelay;

    private bool IsGrounded => (_characterController.isGrounded || _castGround) && _forceUngroundTimer <= 0f;
    public bool IsSurfing { get; private set; }

    public int SurfSide { get; private set; }

    public Vector3 Position => _characterController.transform.position;

    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    private float timer;
    private bool isSurfPlaying;

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
    private bool _wasSurfing;

    private float _surfDebounceTimer;

    private Vector3 _velBeforeMove;

    private void Awake()
    {
        DebugHUD.AddString(DebugString);
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

    public void ForceUnground()
    {
        _forceUngroundTimer = 0.2f;
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

            bool isMovingTowards = Vector3.Dot(hVelocity, desiredHVel) > 0;
            float finalAccel = isMovingTowards
                ? _moveSettings.Acceleration
                : _moveSettings.Friction;

            // Prevent slowing down in the air
            if (!IsGrounded)
            {
                bool noInput = rawInput == Vector2.zero;
                if (noInput)
                {
                    finalAccel = 0;
                }

                if (desiredHVel.sqrMagnitude < hVelocity.sqrMagnitude && isMovingTowards)
                {
                    desiredHVel = desiredHVel.normalized * hVelocity.magnitude;
                }
            }

            Vector3 newHVel = Vector3.Lerp(
                hVelocity,
                desiredHVel,
                finalAccel * Time.deltaTime);

            _velocity += newHVel - hVelocity;
        }

        // Wallsurfing
        bool surfAim = Mathf.Abs(Vector3.Angle(_surfNormal, fpCamera.CameraForward) - 90f) <
                       _moveSettings.SurfInitiateAngleRange;

        bool matchesVelocity = Mathf.Abs(Vector3.Angle(_surfNormal, _velocity.normalized) - 90f) <
                               _moveSettings.SurfInitiateAngleRange;

        IsSurfing = false;
        bool sufficientSpeed = _velocity.magnitude >
                               (_wasSurfing
                                   ? _moveSettings.SurfMaintainSpeedRequirement
                                   : _moveSettings.SurfEnterSpeedRequirement);

        if (!IsGrounded && _surfContact && matchesVelocity && surfAim && sufficientSpeed && _surfDebounceTimer <= 0f)
        {
            Vector3 aim = fpCamera.CameraForward;
            float lookDotWithVel = Vector3.Dot(aim, _velocity.normalized);

            // Allow going backwards
            if (lookDotWithVel < 0f)
            {
                aim *= -1;
            }

            Vector3 desiredDir = Vector3.ProjectOnPlane(aim, _surfNormal).normalized;
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

            IsSurfing = true;
        }
        else if (_wasSurfing)
        {
            _velocity += _surfNormal * _moveSettings.SurfEjectForce;
            _velocity += fpCamera.CameraForward * _moveSettings.SurfEjectForceForward;
            _velocity.y += Mathf.Sqrt(_moveSettings.SurfJumpHeight * -2f * _moveSettings.Gravity);
            _surfDebounceTimer = _moveSettings.SurfDebounceTime;
        }

        _wasSurfing = IsSurfing;

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
                AkSoundEngine.PostEvent("playJump", gameObject);
            }
            else if (IsSurfing)
            {
                _surfDebounceTimer = _moveSettings.SurfDebounceTime;
                AkSoundEngine.PostEvent("playJump", gameObject);
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
        _velBeforeMove = _velocity;
        _characterController.Move(_velocity * Time.deltaTime);
        _velocity = _characterController.velocity;

        //Play footstep sounds
        if (moveInput != Vector3.zero && IsGrounded)
        {
            timer += Time.deltaTime;

            if (timer > footstepDelay)
            {
                AkSoundEngine.PostEvent("playFootstep", gameObject);
                timer = 0f;
            }
        }

        if (IsSurfing && !isSurfPlaying)
        {
            AkSoundEngine.PostEvent("playWallslide", gameObject);
            isSurfPlaying = true;
        }
        else if (!IsSurfing && isSurfPlaying)
        {
            AkSoundEngine.PostEvent("stopWallslide", gameObject);
        }
    }

    private string DebugString()
    {
        return $"Velocity: {_velocity} " +
               $"\n Speed: {_velocity.magnitude}" +
               $"\n VelBefore: {_velBeforeMove}" +
               $"\n Grounded: {IsGrounded}" +
               $"\n Ground Normal: {_groundNormal}" +
               $"\n Did Snap: {_didSnap}" +
               $"\n Surf Contact: {_surfContact}" +
               $"\n Surf Aiming: {IsSurfing}" +
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

        if (Vector3.Angle(hitInfo.normal, Vector3.up) > _moveSettings.MaxGroundAngle)
        {
            hit = false;
        }

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
            SurfSide = hitRight ? 1 : -1;
        }

        float angleFromHorizontal = Mathf.Abs(Vector3.Angle(surfNormal, Vector3.up) - 90);
        bool angleInRange = angleFromHorizontal < _moveSettings.SurfSurfaceAngleRange;

        if (angleInRange)
        {
            _surfNormal = surfNormal;
        }

        _surfContact = hitRight ^ hitLeft && angleInRange;
    }

    public float GetCurrentSpeedFactor()
    {
        return _velocity.magnitude / (_moveSettings.MoveSpeed * 2f);
    }
}