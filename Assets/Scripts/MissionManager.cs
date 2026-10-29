using TMPro;
using UnityEngine;

/// <summary>
/// A simple persistent mission example: collect a target number of coins
/// across runs. Assign the optional UI labels in the Inspector.
/// </summary>
public class MissionManager : MonoBehaviour
{
    [SerializeField, Min(1)] private int coinMissionTarget = 25;
    [SerializeField, Min(1)] private int rewardCoins = 10;
    [SerializeField] private TMP_Text missionText;
    [SerializeField] private TMP_Text missionStatusText;

    private const string CompletedKey = "ColorDashCoinMissionCompleted";
    public static MissionManager Instance { get; private set; }

    private bool completed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        completed = PlayerPrefs.GetInt(CompletedKey, 0) == 1;
    }

    private void Start()
    {
        RefreshUI();
    }

    public void OnCoinCollected(int totalCoins)
    {
        if (completed || totalCoins < coinMissionTarget)
        {
            RefreshUI(totalCoins);
            return;
        }

        completed = true;
        PlayerPrefs.SetInt(CompletedKey, 1);
        PlayerPrefs.Save();

        if (GameManager.Instance != null)
            GameManager.Instance.AddBonusCoins(rewardCoins);

        if (missionStatusText != null)
            missionStatusText.text = "Mission complete! +" + rewardCoins + " coins";

        RefreshUI(totalCoins);
    }

    public void OnScoreChanged(int score)
    {
        // Reserved for adding score/time-based missions later.
    }

    private void RefreshUI(int totalCoins = -1)
    {
        if (totalCoins < 0)
            totalCoins = PlayerPrefs.GetInt("ColorDashLifetimeCoins", 0);

        if (missionText != null)
            missionText.text = completed
                ? "Mission: Completed"
                : "Mission: collect " + coinMissionTarget + " total coins (" +
                  Mathf.Min(totalCoins, coinMissionTarget) + "/" + coinMissionTarget + ")";

        if (missionStatusText != null && !completed)
            missionStatusText.text = "Reward: " + rewardCoins + " coins";
    }

    public void ResetMissionForTesting()
    {
        completed = false;
        PlayerPrefs.DeleteKey(CompletedKey);
        PlayerPrefs.Save();
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
