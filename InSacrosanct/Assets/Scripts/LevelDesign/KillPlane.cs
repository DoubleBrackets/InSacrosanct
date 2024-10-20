using UnityEngine;
using UnityEngine.Timeline;

public class KillPlane : MonoBehaviour
{
    [SerializeField]
    private float _boundary;

    [SerializeField]
    private ServiceLocator _serviceLocator;

    [SerializeField]
    private TimelineAsset _deathTimeline;

    private LocatedService<Protag> _protag;

    private void Awake()
    {
        _protag = new LocatedService<Protag>(_serviceLocator);
    }

    private void Update()
    {
        if (_protag.Instance.Pos.y < _boundary)
        {
            _protag.Instance.Kill(_deathTimeline);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawCube(Vector3.up * _boundary, new Vector3(100, 0.1f, 100));
    }
}