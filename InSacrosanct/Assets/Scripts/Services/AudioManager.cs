using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private float footstepDelay;
    [SerializeField] private GameObject player;
    [SerializeField] private InputProvider inputProvider;
    private float timer;
    void Start()
    {
        AkSoundEngine.PostEvent("playMainMusic", gameObject);
    }

    void Update()
    {
        if (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f)
        {
            timer += Time.deltaTime;

            if (timer > footstepDelay)
            {
                AkSoundEngine.PostEvent("playFootstep", player);
                timer = 0f;
                print("played");
            }
        }

        //AKSoundEngine.PostEvent("playJump", player);
    }
}
