using UnityEngine;
using UnityEngine.Timeline;

public class ContactDeath : MonoBehaviour
{
    [SerializeField]
    private TimelineAsset _deathTimeline;

    private bool _ended;

    private void OnTriggerEnter(Collider other)
    {
        var protag = other.GetComponentInParent<Protag>();
        if (!_ended && protag && enabled)
        {
            _ended = true;
            protag.Kill(_deathTimeline);
        }
    }
}