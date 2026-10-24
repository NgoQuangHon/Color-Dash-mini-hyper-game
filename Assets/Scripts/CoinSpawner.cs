
using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float firstSpawnDistance = 15f;
    [SerializeField] private float rowSpacing = 8f;
    [SerializeField] private float laneRange = 3f;
    [SerializeField] private int coinsPerRow = 3;

    [Header("Cleanup")]
    [SerializeField] private float destroyBehindDistance = 15f;

    private float nextSpawnZ;

    private void Start()
    {
        if (coinPrefab == null || player == null)
        {
            Debug.LogError(
                "CoinSpawner: Hay gan Coin Prefab va Player trong Inspector."
            );
            enabled = false;
            return;
        }

        nextSpawnZ = player.position.z + firstSpawnDistance;
    }

    private void Update()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.IsGameOver)
        {
            return;
        }

        // Sinh các hàng xu ở phía trước Player.
        while (nextSpawnZ <= player.position.z + firstSpawnDistance)
        {
            SpawnCoinRow(nextSpawnZ);
            nextSpawnZ += rowSpacing;
        }

        // Dọn những xu đã bị bỏ lại phía sau.
        CleanupOldCoins();
    }

    private void SpawnCoinRow(float zPosition)
    {
        int count = Mathf.Clamp(coinsPerRow, 1, 3);

        // Các làn: trái, giữa, phải.
        float[] lanes = { -laneRange, 0f, laneRange };

        // Xáo trộn thứ tự làn để hàng xu thay đổi ngẫu nhiên.
        for (int i = 0; i < lanes.Length; i++)
        {
            int randomIndex = Random.Range(i, lanes.Length);
            float temp = lanes[i];
            lanes[i] = lanes[randomIndex];
            lanes[randomIndex] = temp;
        }

        for (int i = 0; i < count; i++)
        {
            Vector3 position = new Vector3(
                lanes[i],
                1f,
                zPosition
            );

            Instantiate(
                coinPrefab,
                position,
                Quaternion.identity
            );
        }
    }

    private void CleanupOldCoins()
    {
        Coin[] coins = FindObjectsByType<Coin>(
            FindObjectsSortMode.None
        );

        foreach (Coin coin in coins)
        {
            if (coin != null &&
                coin.transform.position.z <
                player.position.z - destroyBehindDistance)
            {
                Destroy(coin.gameObject);
            }
        }
    }
}
