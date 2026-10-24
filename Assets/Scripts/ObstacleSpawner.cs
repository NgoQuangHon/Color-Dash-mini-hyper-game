
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnDistance = 35f;
    [SerializeField] private float spawnInterval = 12f;
    [SerializeField] private float laneRange = 3f;

    private float nextSpawnZ;

    private void Start()
    {
        if (player == null || obstaclePrefab == null)
        {
            Debug.LogError(
                "ObstacleSpawner: Chua gan Player hoac Obstacle Prefab!"
            );
            enabled = false;
            return;
        }

        nextSpawnZ = player.position.z + spawnDistance;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
            return;

        if (player.position.z + spawnDistance >= nextSpawnZ)
        {
            SpawnObstacle();
            nextSpawnZ += spawnInterval;
        }
    }

    
private void SpawnObstacle()
{
    float[] lanes = { -laneRange, 0f, laneRange };

    // Chọn ngẫu nhiên 1 hoặc 2 làn bị chặn.
    int obstacleCount = Random.Range(1, 3);

    // Chọn làn đầu tiên.
    int firstLane = Random.Range(0, lanes.Length);

    Vector3 firstPosition = new Vector3(
        lanes[firstLane],
        0.5f,
        nextSpawnZ
    );

    Instantiate(
        obstaclePrefab,
        firstPosition,
        Quaternion.identity
    );

    // Có thể tạo thêm vật cản ở một làn khác.
    if (obstacleCount == 2)
    {
        int secondLane;

        do
        {
            secondLane = Random.Range(0, lanes.Length);
        }
        while (secondLane == firstLane);

        Vector3 secondPosition = new Vector3(
            lanes[secondLane],
            0.5f,
            nextSpawnZ
        );

        Instantiate(
            obstaclePrefab,
            secondPosition,
            Quaternion.identity
        );
    }
}

}
