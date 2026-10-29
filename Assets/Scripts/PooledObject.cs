using UnityEngine;

/// <summary>
/// Internal marker used by ObjectPooler to remember the original prefab.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public GameObject Prefab { get; set; }
}
