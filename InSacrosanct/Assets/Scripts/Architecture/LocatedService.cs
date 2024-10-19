public class LocatedService<T> where T : ILocatableService
{
    public T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = _serviceLocator.Get<T>();
            }

            return _instance;
        }
    }

    private readonly ServiceLocator _serviceLocator;

    private T _instance;

    public LocatedService(ServiceLocator serviceLocator)
    {
        _serviceLocator = serviceLocator;
    }

    public static implicit operator T(LocatedService<T> locatedService)
    {
        return locatedService.Instance;
    }


}