using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Sinh vật thể (xu và vật cản) liên tục theo vị trí Player.
/// Tự động dọn dẹp những vật thể đã ở phía sau Player.
/// Tăng độ khó theo thời gian chơi.
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    // ─── References ──────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject obstaclePrefab;

    // ─── Road ────────────────────────────────────────────────────────────────
    [Header("Road Settings")]
    [SerializeField] private float laneRange      = 3f;
    [SerializeField] private float coinHeight     = 1f;
    [SerializeField] private float obstacleHeight = 0.5f;

    // ─── Coin Trail ──────────────────────────────────────────────────────────
    [Header("Coin Trail")]
    [Tooltip("So xu toi thieu moi day (3-5)")]
    [SerializeField] private int   minCoinsPerTrail = 3;
    [Tooltip("So xu toi da moi day (3-5)")]
    [SerializeField] private int   maxCoinsPerTrail = 5;
    [Tooltip("Khoang cach giua 2 xu lien nhau trong day")]
    [SerializeField] private float coinSpacing      = 2.2f;

    // ─── Spawn Distances ─────────────────────────────────────────────────────
    [Header("Level Spacing")]
    [Tooltip("Khoang cach spawn tu vi tri dau cua Player")]
    [SerializeField] private float firstSpawnDistance = 20f;
    [Tooltip("Luon sinh vat the truoc Player bao nhieu don vi")]
    [SerializeField] private float spawnAheadDistance = 100f;
    [FormerlySerializedAs("gapAfterObstacle")]
    [Tooltip("Khoang cach giua cac hang vat the")]
    [SerializeField] private float rowSpacing         = 12f;
    [Tooltip("Xoa xu khi da xa sau Player bao nhieu don vi")]
    [SerializeField] private float cleanupDistance    = 10f;
    [Tooltip("Xoa vat can khi da xa sau Player bao nhieu don vi")]
    [SerializeField] private float obstacleCleanupDistance = 40f;

    // ─── Difficulty ──────────────────────────────────────────────────────────
    [Header("Difficulty")]
    [Tooltip("Thoi gian (giay) de dat do kho toi da")]
    [SerializeField] private float difficultyRampDuration    = 180f;
    [SerializeField, Range(0f, 1f)] private float initialObstacleChance       = 0.30f;
    [SerializeField, Range(0f, 1f)] private float maxObstacleChance           = 0.85f;
    [SerializeField, Range(0f, 1f)] private float initialDoubleObstacleChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float maxDoubleObstacleChance     = 0.45f;

    // ─── State ───────────────────────────────────────────────────────────────
    private readonly List<GameObject> spawnedCoins     = new List<GameObject>();
    private readonly List<GameObject> spawnedObstacles = new List<GameObject>();
    private readonly float[] lanes = new float[3];

    private float nextSpawnZ;
    private float elapsedPlayTime;
    private int   previousCoinLane     = -1;
    private int   previousObstacleLane = -1;

    // ─── Unity Callbacks ─────────────────────────────────────────────────────
    private void Start()
    {
        if (player == null || coinPrefab == null || obstaclePrefab == null)
        {
            Debug.LogError("[LevelGenerator] Chua gan Player, CoinPrefab hoac ObstaclePrefab trong Inspector!");
            enabled = false;
            return;
        }

        // Clamp tham so hop le
        laneRange                  = Mathf.Max(laneRange, 0.1f);
        lanes[0] = -laneRange;
        lanes[1] =  0f;
        lanes[2] =  laneRange;

        minCoinsPerTrail           = Mathf.Clamp(minCoinsPerTrail, 1, 10);
        maxCoinsPerTrail           = Mathf.Clamp(maxCoinsPerTrail, minCoinsPerTrail, 10);
        coinSpacing                = Mathf.Max(coinSpacing, 0.5f);
        firstSpawnDistance         = Mathf.Max(firstSpawnDistance, 5f);
        spawnAheadDistance         = Mathf.Max(spawnAheadDistance, firstSpawnDistance + 10f);
        rowSpacing                 = Mathf.Max(rowSpacing, (maxCoinsPerTrail - 1) * coinSpacing + 2f);
        cleanupDistance            = Mathf.Max(cleanupDistance, 5f);
        obstacleCleanupDistance    = Mathf.Max(obstacleCleanupDistance, cleanupDistance);
        difficultyRampDuration     = Mathf.Max(difficultyRampDuration, 1f);
        maxObstacleChance          = Mathf.Max(maxObstacleChance, initialObstacleChance);
        maxDoubleObstacleChance    = Mathf.Max(maxDoubleObstacleChance, initialDoubleObstacleChance);

        nextSpawnZ = player.position.z + firstSpawnDistance;
        GenerateAhead();
    }

    private void Update()
    {
        if (player == null) return;
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        elapsedPlayTime += Time.deltaTime;
        GenerateAhead();
        CleanupBehind();
    }

    // ─── Spawn Logic ─────────────────────────────────────────────────────────

    /// <summary>Sinh hang vat the phia truoc cho den khi du khoang buffer.</summary>
    private void GenerateAhead()
    {
        float spawnLimit = player.position.z + spawnAheadDistance;
        while (nextSpawnZ <= spawnLimit)
        {
            SpawnRow(nextSpawnZ);
            nextSpawnZ += rowSpacing;
        }
    }

    /// <summary>Sinh day xu va it nhat mot vat can tai vi tri Z.</summary>
    private void SpawnRow(float zPosition)
    {
        // ── Xu: day 3-5 dong tren mot lan ───────────────────────────────────
        int coinLane  = GetNextCoinLane();
        int coinCount = Random.Range(
            minCoinsPerTrail,
            maxCoinsPerTrail + 1
        );
        float firstCoinZ = zPosition - (coinCount - 1) * coinSpacing * 0.5f;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 position = new Vector3(
                lanes[coinLane],
                coinHeight,
                firstCoinZ + i * coinSpacing
            );
            spawnedCoins.Add(
                Instantiate(coinPrefab, position, Quaternion.identity)
            );
        }

        float difficulty = Mathf.Clamp01(
            elapsedPlayTime / difficultyRampDuration
        );
        float obstacleChance = Mathf.Lerp(
            initialObstacleChance,
            maxObstacleChance,
            difficulty
        );

        // Always place one obstacle so every generated row has a visible obstacle.
        int obstacleLane = GetNextObstacleLane(coinLane);
        SpawnObstacle(obstacleLane, zPosition);

        float doubleObstacleChance = Mathf.Lerp(
            initialDoubleObstacleChance,
            maxDoubleObstacleChance,
            difficulty
        );

        if (Random.value < obstacleChance &&
            Random.value < doubleObstacleChance)
        {
            int secondObstacleLane = 3 - coinLane - obstacleLane;
            SpawnObstacle(secondObstacleLane, zPosition);
            previousObstacleLane = secondObstacleLane;
        }
    }

    private int GetNextCoinLane()
    {
        int lane = Random.Range(0, lanes.Length - 1);
        if (previousCoinLane >= 0 && lane >= previousCoinLane)
            lane++;

        previousCoinLane = lane;
        return lane;
    }

    private int GetNextObstacleLane(int coinLane)
    {
        int firstAvailableLane = (coinLane + 1) % lanes.Length;
        int secondAvailableLane = (coinLane + 2) % lanes.Length;
        int selectedLane;

        if (previousObstacleLane == firstAvailableLane)
            selectedLane = secondAvailableLane;
        else if (previousObstacleLane == secondAvailableLane)
            selectedLane = firstAvailableLane;
        else
            selectedLane = Random.Range(0, 2) == 0
                ? firstAvailableLane
                : secondAvailableLane;

        previousObstacleLane = selectedLane;
        return selectedLane;
    }

    private void SpawnObstacle(int lane, float zPosition)
    {
        Vector3 position = new Vector3(
            lanes[lane],
            obstacleHeight,
            zPosition
        );
        spawnedObstacles.Add(
            Instantiate(obstaclePrefab, position, Quaternion.identity)
        );
    }

    private void CleanupBehind()
    {
        CleanupObjectsBehind(
            spawnedCoins,
            player.position.z - cleanupDistance
        );
        CleanupObjectsBehind(
            spawnedObstacles,
            player.position.z - obstacleCleanupDistance
        );
    }

    private static void CleanupObjectsBehind(
    List<GameObject> objects,
    float cleanupZ)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            GameObject obj = objects[i];

            // Xóa tham chiếu nếu vật thể đã bị hủy ở nơi khác
            if (obj == null)
            {
                objects.RemoveAt(i);
                continue;
            }

            // Chỉ xóa khi vật thể đã đi đủ xa phía sau Player
            if (obj.transform.position.z < cleanupZ)
            {
                Destroy(obj);
                objects.RemoveAt(i);
            }
        }
    }
}
