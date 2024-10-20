using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField]
    private AudioManager.Tracks _track;
    [SerializeField]
    private AK.Wwise.Bank _bank;

    private void Start()
    {
        _bank.Load();
        AudioManager.Instance.PlayMusic(_track);
    }
}