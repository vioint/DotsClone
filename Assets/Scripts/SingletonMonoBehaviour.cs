using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour {

    private static T _instance = null;
    public static T Instance
    {
        get
        {
            return _instance ??
                (_instance = GameObject.FindObjectOfType<T>() ?? new GameObject(typeof(T).Name).AddComponent<T>());
        }
    }

}
