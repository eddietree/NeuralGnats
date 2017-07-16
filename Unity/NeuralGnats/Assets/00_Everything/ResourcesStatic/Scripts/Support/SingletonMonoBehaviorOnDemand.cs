using UnityEngine;
using System.Reflection;

public class SingletonMonoBehaviourOnDemand<T>
    : MonoBehaviour
    where T : Component
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = Initialize();

            return instance;
        }
    }

    public static T Initialize()
    {
        var typename = typeof(T).Name;
        GameObject objInstance = null;

        //var instance = GameObject.Find(typename);
        T instanceTyped = GameObject.FindObjectOfType<T>();
        if (instanceTyped == null)
            objInstance = new GameObject(typename, typeof(T));
        else
            objInstance = instanceTyped.gameObject;

        var component = objInstance.GetComponent<T>();
        if (component == null)
            component = objInstance.AddComponent<T>();

        var flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.FlattenHierarchy;

        var type = typeof(T);
        var method = type.GetMethod("OnSingletonInit", flags);

        // can be null
        if (method != null)
            method.Invoke(component, null);

        return component;
    }

    protected void ForceSetInstanceEarly(T obj)
    {
        instance = obj;
    }
}

