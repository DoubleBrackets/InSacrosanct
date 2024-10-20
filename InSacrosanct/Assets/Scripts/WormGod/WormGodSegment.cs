using System;
using UnityEngine;

public class WormGodSegment : MonoBehaviour
{
    [Serializable]
    public struct SegmentSettings
    {
        public float TerrainCheckRadius;
        public LayerMask TerrainLayer;
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

    private readonly Collider[] _results = new Collider[1];

    private bool _didUpdateInTerrainThisFrame;

    private bool _isInTerrain;

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
}