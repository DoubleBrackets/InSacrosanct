using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WormGodAI : MonoBehaviour
{
    [Serializable]
    private struct Config
    {
        [Header("Animation")]

        public float SegmentDistanceConstraint;

        public float SegmentAngleConstraint;

        public int SegmentCount;

        [Header("Steering")]

        public float SteeringForce;

        [Tooltip("The max angle that the steering force can be away from current vel")]
        public float MaxSteeringAngle;

        public float TopSpeed;

        public float GravityAccel;

        public Vector3 TargetOffset;

        [Header("Screen shake")]

        public float ShakeMaxRange;

        public float ShakeMinRange;
    }

    [Header("Dependencies")]

    [SerializeField]
    private WormGodSegment _segmentPrefab;

    [SerializeField]
    private WormGodSegment _headPrefab;

    [SerializeField]
    private WormGodSegment _tailPrefab;

    [SerializeField]
    private ServiceLocator _serviceLocator;

    [Header("Config")]

    [SerializeField]
    private Config _config;

    private readonly List<WormGodSegment> _bodySegments = new();

    private bool isDead;

    private WormGodSegment _head;

    private LocatedService<Protag> _protag;

    private Vector3 _headVelocity;

    private void Awake()
    {
        _protag = new LocatedService<Protag>(_serviceLocator);
    }

    private void Start()
    {
        InitializeSegments();
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        _head.Tick(_protag.Instance.Pos);
        var finalDeath = true;
        foreach (WormGodSegment segment in _bodySegments)
        {
            segment.Tick(_protag.Instance.Pos);
            if (segment.Killable && segment.Alive)
            {
                finalDeath = false;
            }
        }

        if (finalDeath)
        {
            isDead = true;
            FinalDeath(gameObject.GetCancellationTokenOnDestroy());
        }

        Steering();
        ResolveBodySegments();
        Effects();
    }

    private async UniTaskVoid FinalDeath(CancellationToken ct)
    {
        _protag.Instance.FPCameraEffects.SetWormImpulse(0.5f);
        for (int i = _bodySegments.Count - 1; i >= 0; i--)
        {
            await UniTask.Delay(350);
            ct.ThrowIfCancellationRequested();
            _bodySegments[i].FinalDeath();
        }

        await UniTask.Delay(600);

        _head.FinalDeath();

        await UniTask.Delay(1000);
        _protag.Instance.FPCameraEffects.SetWormImpulse(0);

        Destroy(gameObject);
    }

    private void Effects()
    {
        // Get closest segment to protag
        float closestDist = Vector3.Distance(_head.Pos, _protag.Instance.Pos);
        foreach (WormGodSegment segment in _bodySegments)
        {
            if (!segment.IsInTerrain())
            {
                continue;
            }

            float dist = Vector3.Distance(segment.Pos, _protag.Instance.Pos);
            if (dist < closestDist)
            {
                closestDist = dist;
            }
        }

        float strength = Mathf.InverseLerp(_config.ShakeMaxRange, _config.ShakeMinRange, closestDist);
        _protag.Instance.FPCameraEffects.SetWormImpulse(strength);
    }

    private void InitializeSegments()
    {
        _head = Instantiate(_headPrefab, transform);

        WormGodSegment tail = Instantiate(_tailPrefab, transform);

        for (var i = 0; i < _config.SegmentCount; i++)
        {
            WormGodSegment segment = Instantiate(_segmentPrefab, transform);
            _bodySegments.Add(segment);
        }

        _bodySegments.Add(tail);
    }

    private void Steering()
    {
        bool inGround = _head.IsInTerrain();
        if (inGround)
        {
            Vector3 targetPos = _protag.Instance.Pos + _config.TargetOffset;
            Vector3 dir = (targetPos - _head.Pos).normalized;
            Vector3 desiredVelocity = dir * _config.TopSpeed;

            if (Vector3.Angle(_headVelocity, desiredVelocity) > _config.MaxSteeringAngle)
            {
                desiredVelocity = Vector3.RotateTowards(
                    _headVelocity,
                    desiredVelocity,
                    _config.MaxSteeringAngle * Mathf.Deg2Rad,
                    0);
            }

            Vector3 newVel = Vector3.MoveTowards(
                _headVelocity,
                desiredVelocity,
                _config.SteeringForce * Time.deltaTime);

            _headVelocity = newVel;
        }
        else
        {
            _headVelocity += Vector3.up * (_config.GravityAccel * Time.deltaTime);
        }

        _head.Pos += _headVelocity * Time.deltaTime;
    }

    private void ResolveBodySegments()
    {
        WormGodSegment prevSegment = _head;
        float distConstraint = _config.SegmentDistanceConstraint;
        for (var i = 0; i < _bodySegments.Count; i++)
        {
            Vector3 prevPos = prevSegment.Pos;
            WormGodSegment currentSegment = _bodySegments[i];
            Vector3 vectorTo = prevPos - currentSegment.Pos;

            if (vectorTo.sqrMagnitude > distConstraint * distConstraint)
            {
                currentSegment.Pos = prevPos - vectorTo.normalized * distConstraint;
            }

            prevSegment = currentSegment;
        }
    }
}