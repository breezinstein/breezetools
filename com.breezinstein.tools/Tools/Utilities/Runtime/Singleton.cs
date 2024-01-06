using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _instanceLock = new object();
    private static bool _quitting = false;

    public static T Instance
    {
        get
        {
            lock (_instanceLock)
            {
                if (_instance == null && !_quitting)
                {
                    T tempInstance = GameObject.FindObjectOfType<T>();
                    if (tempInstance == null)
                    {
                        GameObject go = new GameObject(nameof(T));
                        _instance = go.AddComponent<T>();
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    else
                    {
                        _instance = tempInstance;
                    }
                }

                return _instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = gameObject.GetComponent<T>();
        }
        else
        {
            int instanceID = GetInstanceID();
            if (_instance.GetInstanceID() != instanceID)
            {
                Destroy(gameObject);
                throw new System.Exception($"Instance of {GetType().FullName} already exists, removing {ToString()}");
            }
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _quitting = true;
    }
}
