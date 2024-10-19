using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;

public class LevelFinish : MonoBehaviour
{
    [SerializeField]
    private ServiceLocator _serviceLocator;
    
    [SerializeField]
    private string _nextLevelName;

    [SerializeField]
    private PlayableDirector _director;

    private bool _ended = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!_ended && other.GetComponentInParent<Protag>())
        {
            _ended = true;
            EndLevel(gameObject.GetCancellationTokenOnDestroy()).Forget();
        }
    }

    private async UniTaskVoid EndLevel(CancellationToken token)
    {
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
        
        _serviceLocator.Get<LevelLoader>().LoadLevelAsync(_nextLevelName);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}
