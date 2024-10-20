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

    private void OnDrawGizmos()
    {
        Gizmos.color = IsInTerrain() ? Color.red : Color.green;
        Gizmos.DrawWireSphere(Pos, _settings.TerrainCheckRadius);
    }

    public bool IsInTerrain()
    {
        int size = Physics.OverlapSphereNonAlloc(Pos, _settings.TerrainCheckRadius, _results, _settings.TerrainLayer);

        return size > 0 || Pos.y < 0;
    }
}