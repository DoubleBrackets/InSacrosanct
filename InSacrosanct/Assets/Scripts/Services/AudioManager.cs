using UnityEngine;

public class AudioManager : MonoBehaviour
{
    void Start()
    {
        AkSoundEngine.PostEvent("playMusic", gameObject);
    }
}
