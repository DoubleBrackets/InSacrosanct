using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioManager.Tracks _track;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(_track);
    }
}