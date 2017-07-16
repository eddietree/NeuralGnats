using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtension
{
    public static T InstantiateDefaultObject<T>(string path)
        where T : Component
    {
        var resource = Resources.Load<GameObject>(path);
        if (resource == null)
        {
            Debug.LogErrorFormat("Instantiate: invalid prefab '{0}'", path);
            return null;
        }

        var instance = GameObject.Instantiate(resource, Vector3.zero, Quaternion.identity) as GameObject;
        var component = instance.GetComponent<T>();

        return component;
    }

    public static GameObject InstantiatePrefab(string path)
    {
        var fullPath = "Prefabs/" + path;
        var resource = Resources.Load<GameObject>(fullPath);
        if (resource == null)
        {
            Debug.LogErrorFormat("Instantiate: invalid prefab '{0}'", fullPath);
            return null;
        }

        var instance = GameObject.Instantiate(resource, Vector3.zero, Quaternion.identity) as GameObject;
        return instance;
    }

    public static T InstantiatePrefabTyped<T>(string path)
        where T : Component
    {
        var inst = InstantiatePrefab(path);
        return inst.GetComponent<T>();
    }

    public static T EnsureComponent<T>(this GameObject obj)
          where T : Component
    {
        var component = obj.GetComponent<T>();

        if (obj.GetComponent<T>() == null)
            return obj.AddComponent<T>();

        return component;
    }

    public static void StopAndNullify(this MonoBehaviour obj, ref Coroutine thread)
    {
        if (thread != null)
        {
            obj.StopCoroutine(thread);
            thread = null;
        }
    }

    public static GameObject CreatePrimitiveNoCollision(PrimitiveType type)
    {
        var result = GameObject.CreatePrimitive(type);
        result.GetComponent<Collider>().enabled = false;
        return result;
    }
}