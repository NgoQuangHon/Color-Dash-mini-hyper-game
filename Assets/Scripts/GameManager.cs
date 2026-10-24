using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Start,
        Playing,
        Paused,
        GameOver
    }

    [Header("UI Panels")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Gameplay UI Text")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinCountText;
    [SerializeField] private TMP_Text highScoreText;

    [Header("Game Over UI Text")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text gameOverHighScoreText;

    [Header("Gameplay")]
    [SerializeField] private GameObject player;
    [SerializeField] private LevelGenerator levelGenerator;
    [SerializeField] private int pointsPerSecond = 1;
    [SerializeField] private int pointsPerCoin = 10;

    private GameState currentState = GameState.Start;
    private float elapsedTime;
    private int coinScore;
    private int coinsCollected;
    private int highScore;

    public static GameManager Instance { get; private set; }
    public GameState CurrentState => currentState;
    public bool IsPlaying => currentState == GameState.Playing;
    public bool IsGameOver => currentState == GameState.GameOver;

    private int CurrentScore =>
        Mathf.FloorToInt(elapsedTime) * pointsPerSecond + coinScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Co nhieu GameManager trong Scene!");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        highScore = PlayerPrefs.GetInt("ColorDashHighScore", 0);
        Time.timeScale = 1f;
    }

    private void Start()
    {
        elapsedTime = 0f;
        coinScore = 0;
        coinsCollected = 0;

        if (levelGenerator != null)
            levelGenerator.enabled = false;

        SetPanel(startPanel, true);
        SetPanel(pausePanel, false);
        SetPanel(gameOverPanel, false);

        if (player != null)
            player.SetActive(false);

        UpdateScoreUI();
        UpdateCoinUI();
        UpdateHighScoreUI();

        currentState = GameState.Start;
        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        elapsedTime += Time.deltaTime;
        UpdateScoreUI();
    }

    public void StartGame()
    {
        if (currentState != GameState.Start)
            return;

        SetPanel(startPanel, false);
        SetPanel(pausePanel, false);
        SetPanel(gameOverPanel, false);

        currentState = GameState.Playing;
        Time.timeScale = 1f;

        if (player != null)
            player.SetActive(true);

        if (levelGenerator != null)
            levelGenerator.enabled = true;

        UpdateScoreUI();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Paused;
        Time.timeScale = 0f;
        SetPanel(pausePanel, true);
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused)
            return;

        SetPanel(pausePanel, false);
        currentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOver()
    {
        if (currentState == GameState.GameOver ||
            currentState == GameState.Start)
            return;

        currentState = GameState.GameOver;
        Time.timeScale = 0f;

        if (levelGenerator != null)
            levelGenerator.enabled = false;

        int score = CurrentScore;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("ColorDashHighScore", highScore);
            PlayerPrefs.Save();
        }

        SetPanel(pausePanel, false);
        SetPanel(gameOverPanel, true);

        if (finalScoreText != null)
        {
            finalScoreText.text = finalScoreText == gameOverHighScoreText
                ? "GAME OVER\nScore: " + score + "\nBest: " + highScore
                : "GAME OVER\nScore: " + score;
        }

        if (gameOverHighScoreText != null &&
            gameOverHighScoreText != finalScoreText)
            gameOverHighScoreText.text = "Best: " + highScore;

        UpdateHighScoreUI();
    }

    public void AddCoin()
    {
        if (currentState != GameState.Playing)
            return;

        coinsCollected++;
        coinScore += pointsPerCoin;

        UpdateScoreUI();
        UpdateCoinUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + CurrentScore;
    }

    private void UpdateCoinUI()
    {
        if (coinCountText != null)
            coinCountText.text = "Coins: " + coinsCollected;
    }

    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore;
    }

    private void SetPanel(GameObject panel, bool visible)
    {
        if (panel != null)
            panel.SetActive(visible);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        Time.timeScale = 1f;
    }
}
