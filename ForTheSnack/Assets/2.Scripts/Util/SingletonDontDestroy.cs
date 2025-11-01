using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonDontDestroy<T> : MonoBehaviour where T : SingletonDontDestroy<T>
{
    public static T Instance { get; private set; }
    protected virtual void OnStart() { }
    protected virtual void OnAwake() { }
    protected virtual void Enable() { }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = (T)this;
            OnAwake();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    protected virtual void Start()
    {
        if (Instance == (T)this)
        {
            OnStart();
        }
    }

    protected virtual void OnEnable()
    {
        if (Instance == (T)this)
        {
            Enable();
        }
    }
}
