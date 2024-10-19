using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour, ILocatableService
{
    [SerializeField]
    private ServiceLocator _serviceLocator;

    private void Awake()
    {
        if (_serviceLocator.Has<LevelLoader>())
        {
            return;
        }

        _serviceLocator.Register(this);
    }

    private void OnDestroy()
    {
        _serviceLocator.Deregister(this);
    }

    public async UniTask LoadLevelAsync(string levelName)
    {
        if (string.IsNullOrEmpty(levelName))
        {
            Debug.LogError("Level name is null or empty.");
            return;
        }

        await SceneManager.LoadSceneAsync(levelName);
    }

    public async UniTask ReloadLevelAsync()
    {
        await SceneManager.LoadSceneAsync(
            SceneManager.GetActiveScene().name);
    }
}