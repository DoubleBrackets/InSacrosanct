using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class LevelEntry : MonoBehaviour
{
    [SerializeField]
    private PlayableDirector _director;
    
    [SerializeField]
    private ServiceLocator _serviceLocator;
    
    private LocatedService<Protag> _protag;

    private void Awake()
    {
        _protag = new LocatedService<Protag>(_serviceLocator);
    }

    private void Start()
    {
        StartLevel(gameObject.GetCancellationTokenOnDestroy()).Forget();
    }
    
    private async UniTaskVoid StartLevel(CancellationToken token)
    {
        _protag.Instance.SetToCinematicState(true);
        if (_director != null)
        {
            _director.Play();
            while (_director.state == PlayState.Playing)
            {
                token.ThrowIfCancellationRequested();
                await UniTask.Yield();
            }
        }
        
        token.ThrowIfCancellationRequested();
        
        _protag.Instance.SetToCinematicState(false);
    }
}
