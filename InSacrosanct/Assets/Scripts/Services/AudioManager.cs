using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum Tracks
    {
        None,
        Main,
        Intro
    }

    public static AudioManager Instance { get; private set; }

    private uint musicEventId;

    private Tracks currentTrack;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayMusic(Tracks track)
    {
        if (currentTrack == track)
        {
            return;
        }

        if (musicEventId != 0)
        {
            AkSoundEngine.StopPlayingID(musicEventId);
        }

        switch (track)
        {
            case Tracks.Main:
                musicEventId = AkSoundEngine.PostEvent("playMainMusic", gameObject);
                break;
            case Tracks.Intro:
                musicEventId = AkSoundEngine.PostEvent("playIntroMusic", gameObject);
                break;
        }
    }

    public void StopMusic()
    {
        if (musicEventId != 0)
        {
            AkSoundEngine.StopPlayingID(musicEventId);
            currentTrack = Tracks.None;
        }
    }
}