using UnityEngine;

public class ServiceInstantiator : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefab;

    public static ServiceInstantiator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Instantiate(_prefab, null);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}