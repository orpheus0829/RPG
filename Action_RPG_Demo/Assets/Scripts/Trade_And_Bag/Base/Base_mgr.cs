using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base_mgr<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T instance { get; private set; }
    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
