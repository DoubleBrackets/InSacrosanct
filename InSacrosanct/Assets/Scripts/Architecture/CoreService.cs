using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CoreService : MonoBehaviour, ILocatableService
{
    [SerializeField]
    private ServiceLocator _serviceLocator;

    [SerializeField]
    private LevelLoader _levelLoader;

    [SerializeField]
    private InputProvider _inputProvider;

    [SerializeField]
    private PlayableDirector _deathDirector;

    public int CurrentLevel { get; set; }

    public event Action<float> CoreUpdate;
    public event Action<float> CoreFixedUpdate;

    private void Awake()
    {
        if (_serviceLocator.Has<CoreService>())
        {
            return;
        }

        Application.targetFrameRate = 60;

        _serviceLocator.Register(this);
    }

    private void Update()
    {
        CoreUpdate?.Invoke(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        CoreFixedUpdate?.Invoke(Time.fixedDeltaTime);
    }

    private void OnDestroy()
    {
        _serviceLocator.Deregister(this);
    }

    public void Death(TimelineAsset deathTimeline)
    {
        HandleDeath(deathTimeline, gameObject.GetCancellationTokenOnDestroy()).Forget();
    }

    private async UniTask HandleDeath(TimelineAsset deathTimeline, CancellationToken token)
    {
        try
        {
            _deathDirector.gameObject.SetActive(true);
            _deathDirector.playableAsset = deathTimeline;
            _deathDirector.Play();

            while (_deathDirector.state == PlayState.Playing)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }

            token.ThrowIfCancellationRequested();

            await _levelLoader.ReloadLevelAsync();

            token.ThrowIfCancellationRequested();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _deathDirector.Stop();
            _deathDirector.gameObject.SetActive(false);
        }
    }

    public void LoadLevel(string nextLevelName)
    {
        _levelLoader.LoadLevelAsync(nextLevelName).Forget();
    }
}