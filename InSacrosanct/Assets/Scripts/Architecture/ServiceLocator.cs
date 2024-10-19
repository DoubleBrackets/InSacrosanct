using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = System.Object;

[CreateAssetMenu(fileName = "ServiceLocator", menuName = "ServiceLocator")]
public class ServiceLocator : ScriptableObject
{
    [ShowInInspector]
    private readonly Dictionary<Type, Object> _registeredServices = new();

    public void Register<T>(T service) where T : ILocatableService
    {
        _registeredServices[typeof(T)] = service;
    }

    public T Get<T>() where T : ILocatableService
    {
        if (_registeredServices.TryGetValue(typeof(T), out Object service))
        {
            return (T)service;
        }

        Debug.LogWarning($"Service of type {typeof(T)} not found. Was it registered?");
        return default;
    }

    public bool Has<T>() where T : ILocatableService
    {
        return _registeredServices.ContainsKey(typeof(T));
    }

    public void Deregister<T>(T self) where T : ILocatableService
    {
        var current = Get<T>();
        if (current as Object != self as Object)
        {
            return;
        }

        _registeredServices.Remove(typeof(T));
    }
}

public interface ILocatableService
{
}