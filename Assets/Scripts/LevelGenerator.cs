using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject obstaclePrefab;

    [Header("Road / Lanes")]
    [Tooltip("Overridden by PlayerController.LaneRange when the Player has that component.")]
    [SerializeField, Min(0.1f)] private float laneRange = 3f;
    [SerializeField] private float coinHeight = 1f;
    [SerializeField] private float obstacleHeight = 0.5f;

    [Header("Coin Trails")]
    [SerializeField, Range(1, 10)] private int minCoinsPerTrail = 3;
    [SerializeField, Range(1, 10)] private int maxCoinsPerTrail = 5;
    [SerializeField, Min(0.5f)] private float coinSpacing = 2.2f;

    [Header("Spawn Distances")]
    [SerializeField, Min(5f)] private float firstSpawnDistance = 20f;
    [SerializeField, Min(15f)] private float spawnAheadDistance = 100f;
    [SerializeField, Min(5f)] private float rowSpacing = 14f;
    [SerializeField, Min(5f)] private float cleanupDistance = 12f;
    [SerializeField, Min(5f)] private float obstacleCleanupDistance = 35f;

    [Header("Difficulty")]
    [SerializeField, Min(1f)] private float difficultyRampDuration = 180f;
    [SerializeField, Range(0f, 1f)] private float initialObstacleChance = 0.45f;
    [SerializeField, Range(0f, 1f)] private float maxObstacleChance = 0.85f;
    [SerializeField, Range(0f, 1f)] private float initialDoubleObstacleChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float maxDoubleObstacleChance = 0.35f;

    private readonly List<GameObject> spawnedCoins = new List<GameObject>();
    private readonly List<GameObject> spawnedObstacles = new List<GameObject>();
    private readonly float[] lanes = new float[3];

    private float nextSpawnZ;
    private float elapsedPlayTime;
    private int previousCoinLane = -1;

    private void Start()
    {
        if (player == null || coinPrefab == null || obstaclePrefab == null)
        {
            Debug.LogError("[LevelGenerator] Assign Player, Coin Prefab and Obstacle Prefab in Inspector.");
            enabled = false;
            return;
        }

        // FIX: use one single source of truth for lane width.
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
            laneRange = playerController.LaneRange;

        laneRange = Mathf.Max(0.1f, laneRange);
        lanes[0] = -laneRange;
        lanes[1] = 0f;
        lanes[2] = laneRange;

        minCoinsPerTrail = Mathf.Clamp(minCoinsPerTrail, 1, 10);
        maxCoinsPerTrail = Mathf.Clamp(maxCoinsPerTrail, minCoinsPerTrail, 10);
        coinSpacing = Mathf.Max(0.5f, coinSpacing);
        rowSpacing = Mathf.Max(rowSpacing, (maxCoinsPerTrail - 1) * coinSpacing + 3f);

        // FIX: guarantee that the player can physically move one lane between
        // two rows even at max speed (one lane = laneRange units).
        if (playerController != null && playerController.LateralSpeed > 0.01f)
        {
            float secondsPerLane = laneRange / playerController.LateralSpeed;
            float minRowSpacing = playerController.MaxForwardSpeed * secondsPerLane + 1f;
            rowSpacing = Mathf.Max(rowSpacing, minRowSpacing);
        }

        spawnAheadDistance = Mathf.Max(spawnAheadDistance, firstSpawnDistance + rowSpacing);
        obstacleCleanupDistance = Mathf.Max(obstacleCleanupDistance, cleanupDistance);
        difficultyRampDuration = Mathf.Max(1f, difficultyRampDuration);
        maxObstacleChance = Mathf.Max(initialObstacleChance, maxObstacleChance);
        maxDoubleObstacleChance = Mathf.Max(initialDoubleObstacleChance, maxDoubleObstacleChance);

        nextSpawnZ = player.position.z + firstSpawnDistance;
        GenerateAhead();
    }

    private void Update()
    {
        if (player == null || (GameManager.Instance != null && !GameManager.Instance.IsPlaying))
            return;

        elapsedPlayTime += Time.deltaTime;
        GenerateAhead();
        CleanupBehind();
    }

    private void GenerateAhead()
    {
        float spawnLimit = player.position.z + spawnAheadDistance;
        int safetyCounter = 0;

        while (nextSpawnZ <= spawnLimit && safetyCounter++ < 100)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += rowSpacing;
        }
    }

    private void SpawnRow(float z)
    {
        int lastCoinLane = previousCoinLane;
        int coinLane = GetNextCoinLane();

        // A jump of two lanes (edge -> opposite edge) is the hardest move.
        bool wideLaneJump = lastCoinLane >= 0 && Mathf.Abs(coinLane - lastCoinLane) > 1;

        int coinCount = Random.Range(minCoinsPerTrail, maxCoinsPerTrail + 1);
        float firstCoinZ = z - (coinCount - 1) * coinSpacing * 0.5f;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 coinPosition = new Vector3(lanes[coinLane], coinHeight, firstCoinZ + i * coinSpacing);
            spawnedCoins.Add(SpawnObject(coinPrefab, coinPosition));
        }

        float difficulty = Mathf.Clamp01(elapsedPlayTime / difficultyRampDuration);
        float obstacleChance = Mathf.Lerp(initialObstacleChance, maxObstacleChance, difficulty);
        if (Random.value > obstacleChance)
            return;

        // The coin lane remains open. One or two lanes may contain obstacles,
        // so there is always at least one safe lane.
        List<int> openObstacleLanes = new List<int>(2);
        for (int lane = 0; lane < lanes.Length; lane++)
        {
            if (lane != coinLane)
                openObstacleLanes.Add(lane);
        }

        int firstIndex = Random.Range(0, openObstacleLanes.Count);
        int firstObstacleLane = openObstacleLanes[firstIndex];
        spawnedObstacles.Add(SpawnObject(
            obstaclePrefab,
            new Vector3(lanes[firstObstacleLane], obstacleHeight, z)
        ));
        openObstacleLanes.RemoveAt(firstIndex);

        // FIX: never block two lanes right after a two-lane jump, otherwise the
        // only safe lane can be unreachable from the previous row's safe lane.
        float doubleChance = wideLaneJump
            ? 0f
            : Mathf.Lerp(initialDoubleObstacleChance, maxDoubleObstacleChance, difficulty);

        if (openObstacleLanes.Count > 0 && Random.value < doubleChance)
        {
            int secondLane = openObstacleLanes[0];
            spawnedObstacles.Add(SpawnObject(
                obstaclePrefab,
                new Vector3(lanes[secondLane], obstacleHeight, z)
            ));
        }
    }

    private int GetNextCoinLane()
    {
        int lane = Random.Range(0, lanes.Length);
        if (lanes.Length > 1 && lane == previousCoinLane)
            lane = (lane + Random.Range(1, lanes.Length)) % lanes.Length;

        previousCoinLane = lane;
        return lane;
    }

    private static GameObject SpawnObject(GameObject prefab, Vector3 position)
    {
        if (ObjectPooler.Instance != null)
            return ObjectPooler.Instance.Get(prefab, position, Quaternion.identity);

        return Instantiate(prefab, position, Quaternion.identity);
    }

    private void CleanupBehind()
    {
        CleanupList(spawnedCoins, player.position.z - cleanupDistance);
        CleanupList(spawnedObstacles, player.position.z - obstacleCleanupDistance);
    }

    private static void CleanupList(List<GameObject> objects, float cleanupZ)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            GameObject obj = objects[i];
            if (obj == null)
            {
                objects.RemoveAt(i);
                continue;
            }

            // FIX: a collected coin is already released to the pool (inactive).
            // Drop it from the tracking list instead of releasing it again.
            if (!obj.activeSelf)
            {
                objects.RemoveAt(i);
                continue;
            }

            if (obj.transform.position.z < cleanupZ)
            {
                if (ObjectPooler.Instance != null)
                    ObjectPooler.Instance.Release(obj);
                else
                    Destroy(obj);

                objects.RemoveAt(i);
            }
        }
    }
}
