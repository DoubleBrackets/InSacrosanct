using System;
using UnityEngine;

public class WormGodSegment : MonoBehaviour, IKillable
{
    [Serializable]
    public struct SegmentSettings
    {
        public float TerrainCheckRadius;
        public LayerMask TerrainLayer;
        public bool Killable;
        public ParticleSystem DeathParticles;
        public GameObject AliveVisuals;
        public GameObject DeadVisuals;
        public ParticleSystem FinalDeathParticles;
        public Transform LookAtTarget;
    }

    [SerializeField]
    private Transform _anchorPoint;

    [SerializeField]
    private SegmentSettings _settings;

    public Vector3 Pos
    {
        get => transform.position;
        set => transform.position = value;
    }

    public bool Alive { get; private set; } = true;

    public Quaternion Rotation
    {
        get => transform.rotation;
        set => transform.rotation = value;
    }

    private readonly Collider[] _results = new Collider[1];

    private bool _didUpdateInTerrainThisFrame;

    private bool _isInTerrain;

    private void Awake()
    {
        _settings.AliveVisuals.SetActive(true);
        _settings.DeadVisuals.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = IsInTerrain() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(Pos, _settings.TerrainCheckRadius);
    }

    public bool Killable => _settings.Killable;

    public void Kill()
    {
        if (!_settings.Killable)
        {
            return;
        }

        Alive = false;

        _settings.AliveVisuals.SetActive(false);
        _settings.DeadVisuals.SetActive(true);

        if (_settings.DeathParticles != null)
        {
            _settings.DeathParticles.Play();
        }
    }

    public Transform AnchorPoint => _anchorPoint;

    public bool IsInTerrain()
    {
        if (_didUpdateInTerrainThisFrame)
        {
            return _isInTerrain;
        }

        _didUpdateInTerrainThisFrame = true;
        int size = Physics.OverlapSphereNonAlloc(Pos, _settings.TerrainCheckRadius, _results,
            _settings.TerrainLayer);

        _isInTerrain = size > 0 || Pos.y < 0;

        return _isInTerrain;
    }

    public void Tick(Vector3 target)
    {
        _didUpdateInTerrainThisFrame = false;

        if (Alive && _settings.LookAtTarget != null)
        {
            _settings.LookAtTarget.LookAt(target);
        }
    }

    public void FinalDeath()
    {
        _settings.FinalDeathParticles.Play();
        _settings.DeadVisuals.SetActive(false);
        _settings.AliveVisuals.SetActive(false);
        AnchorPoint.gameObject.SetActive(false);
    }
}