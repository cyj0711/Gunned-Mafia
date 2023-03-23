using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SingletonPunCallbacks<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T _instance;
    public static T I
    {
        get
        {
            if (null == _instance)
            {
                _instance = FindObjectOfType(typeof(T)) as T;
                if (null == _instance)
                {
                    GameObject managerGameObj = new GameObject("SINGLETON_INSTANCE");
                    _instance = managerGameObj.AddComponent<T>();
                }
            }
            return _instance;
        }
        protected set { _instance = value; }
    }

    public bool dontDestroyOnLoad = false;

    protected virtual void Awake()
    {
        if (null != _instance)
        {
            Debug.LogError("Singleton object must be only one");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }
}
