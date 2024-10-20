using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AudioManager : MonoBehaviour
{
    
    void Start()
    {
        AkSoundEngine.PostEvent("playMainMusic", gameObject);
    }
}
