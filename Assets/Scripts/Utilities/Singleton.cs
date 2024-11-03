using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DefaultExecutionOrder(-1)]
public class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    public static T instance;

    private bool isPersistant = false;

    protected virtual void Awake()
    {
        if (isPersistant)
        {
            if (instance)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            instance = this as T;
        }
    }
}