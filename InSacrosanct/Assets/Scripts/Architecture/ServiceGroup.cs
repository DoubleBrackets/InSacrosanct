using System;
using UnityEngine;

public class ServiceGroup : MonoBehaviour
{
    public static ServiceGroup Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null)
        {
            gameObject.SetActive(false);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
