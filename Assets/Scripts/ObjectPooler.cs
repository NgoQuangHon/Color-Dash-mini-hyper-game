using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Optional lightweight pool. Add this component to one active scene object.
/// LevelGenerator and Coin use it automatically when it exists.
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    private readonly Dictionary<GameObject, Stack<GameObject>> pools =
        new Dictionary<GameObject, Stack<GameObject>>();

    public static ObjectPooler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Only one ObjectPooler should exist in a scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
            return null;

        if (!pools.TryGetValue(prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            pools.Add(prefab, pool);
        }

        GameObject instance = null;
        while (pool.Count > 0 && instance == null)
            instance = pool.Pop();

        if (instance == null)
        {
            instance = Instantiate(prefab);
            PooledObject marker = instance.GetComponent<PooledObject>();
            if (marker == null)
                marker = instance.AddComponent<PooledObject>();
            marker.Prefab = prefab;
        }

        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
            return;

        PooledObject marker = instance.GetComponent<PooledObject>();
        if (marker == null || marker.Prefab == null)
        {
            Destroy(instance);
            return;
        }

        // FIX: an object that is already inactive is already in the pool
        // (e.g. a collected coin that LevelGenerator later tries to clean up).
        // Pushing it again would put the same object in the stack twice and
        // make two spawns share one instance.
        if (!instance.activeSelf)
            return;

        if (!pools.TryGetValue(marker.Prefab, out Stack<GameObject> pool))
        {
            pool = new Stack<GameObject>();
            pools.Add(marker.Prefab, pool);
        }

        instance.SetActive(false);
        pool.Push(instance);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
