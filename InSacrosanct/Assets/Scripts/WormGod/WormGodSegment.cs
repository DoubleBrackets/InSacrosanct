using System;
using UnityEngine;

public class WormGodSegment : MonoBehaviour
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
    }

    [SerializeField]
    private Transform _anchorPoint;

    [SerializeField]
    private SegmentSettings _settings;

    public bool Killable => _settings.Killable;

    public Vector3 Pos
    {
        get => transform.position;
        set => transform.position = value;
    }

    public bool Alive { get; private set; } = true;

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

    public void Tick()
    {
        _didUpdateInTerrainThisFrame = false;
    }

    public void Kill()
    {
        if (!_settings.Killable)
        {
            return;
        }

        Alive = false;

        _settings.AliveVisuals.SetActive(false);
        _settings.DeadVisuals.SetActive(true);
        _settings.DeathParticles.Play();
    }

    public void FinalDeath()
    {
        _settings.FinalDeathParticles.Play();
        _settings.DeadVisuals.SetActive(false);
    }
}