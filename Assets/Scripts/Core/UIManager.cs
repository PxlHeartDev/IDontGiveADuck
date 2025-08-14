using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Manages all UI elements and updates them based on game state
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI progressText;
    
    [Header("Game Over Panel")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverTitle;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    
    [Header("Instructions Panel")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button testLevel12Button;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color timerWarningColor = Color.red;
    [SerializeField] private float timerWarningThreshold = 10f;
    
    private Color originalTimerColor;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        if (timerText != null)
            originalTimerColor = timerText.color;
        
        SetupButtonListeners();
    }
    
    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTimer;
            GameManager.Instance.OnGameStateChanged += UpdateGameState;
            GameManager.Instance.OnLevelLoaded += UpdateLevelInfo;
        }
        
        HideHUDElements();
        ShowInstructions();
    }
    
    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged -= UpdateScore;
            GameManager.Instance.OnLivesChanged -= UpdateLives;
            GameManager.Instance.OnTimeChanged -= UpdateTimer;
            GameManager.Instance.OnGameStateChanged -= UpdateGameState;
            GameManager.Instance.OnLevelLoaded -= UpdateLevelInfo;
        }
    }
    
    #endregion
    
    #region Setup
    
    private void SetupButtonListeners()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnRestartClicked);
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
        if (testLevel12Button != null)
            testLevel12Button.onClick.AddListener(OnTestLevel12Clicked);
    }
    
    #endregion
    
    #region HUD Updates
    
    public void UpdateScore(int score)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score:N0}";
    }
    
    public void UpdateTimer(float timeLeft)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            if (timeLeft <= timerWarningThreshold)
                timerText.color = Color.Lerp(timerWarningColor, originalTimerColor, timeLeft / timerWarningThreshold);
            else
                timerText.color = originalTimerColor;
        }
    }
    
    public void UpdateLives(int lives)
    {
        if (livesText != null)
            livesText.text = $"Lives: {lives}";
    }
    
    public void UpdateLevelInfo(LevelData levelData)
    {
        if (levelText != null)
            levelText.text = $"Level: {levelData.levelId}";
        
        UpdateProgress();
    }
    
    public void UpdateProgress()
    {
        if (progressText != null && GameManager.Instance != null)
        {
            int clicked = GameManager.Instance.GoodDucksClicked;
            int required = GameManager.Instance.GoodDucksRequired;
            progressText.text = $"Progress: {clicked}/{required}";
        }
    }
    
    #endregion
    
    #region Game State Updates
    
    public void UpdateGameState(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                ShowInstructions();
                break;
            case GameState.Playing:
                ShowGameHUD();
                break;
            case GameState.Paused:
                ShowPausePanel();
                break;
            case GameState.LevelComplete:
                ShowLevelComplete();
                break;
            case GameState.GameOver:
                ShowGameOver(false);
                break;
            case GameState.GameComplete:
                ShowGameComplete();
                break;
        }
    }
    
    #endregion
    
    #region HUD Visibility Control
    
    private void HideHUDElements()
    {
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
    }
    
    private void ShowHUDElements()
    {
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (livesText != null) livesText.gameObject.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (progressText != null) progressText.gameObject.SetActive(true);
    }
    
    #endregion
    
    #region Panel Management
    
    private void ShowInstructions()
    {
        SetAllPanelsInactive();
        HideHUDElements();
        
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
        }
        else
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame(true);
        }
    }
    
    private void ShowGameHUD()
    {
        SetAllPanelsInactive();
        ShowHUDElements();
        UpdateProgress();
    }
    
    private void ShowPausePanel()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    private void ShowLevelComplete()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverTitle != null)
                gameOverTitle.text = "Level Complete!";

            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";

            if (nextLevelButton != null)
            {
                int nextLevel = LevelLoader.Instance?.GetNextLevelId(GameManager.Instance.CurrentLevelId) ?? -1;
                nextLevelButton.gameObject.SetActive(nextLevel > 0);

                var tmpText = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                    tmpText.text = nextLevel > 0 ? $"Next Level ({nextLevel})" : "Next Level";
            }
        }
    }
    
    private void ShowGameOver(bool isCompleteGameOver)
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitle != null)
                gameOverTitle.text = "Level Failed!";
            
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = "Restart from Level 1";
            
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
        }
    }
    
    private void ShowGameComplete()
    {
        HideHUDElements();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitle != null)
                gameOverTitle.text = "Game Complete";
            
            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";
            
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
        }
    }
    
    private void SetAllPanelsInactive()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }
    
    #endregion
    
    #region Button Handlers
    
    private void OnStartGameClicked()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        
        if (GameManager.Instance != null)
            GameManager.Instance.StartGame(true);
    }
    
    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartLevel();
    }
    
    private void OnNextLevelClicked()
    {
        GameManager.Instance?.AdvanceToNextLevel();
    }
    
    private void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
    }
    
    private void OnTestLevel12Clicked()
    {
        if (instructionsPanel != null)
            instructionsPanel.SetActive(false);
        
        if (GameManager.Instance != null)
            GameManager.Instance.JumpToLevel(12);
    }
    
    #endregion
    
    #region Input Handling
    
    void Update()
    {
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            if (GameManager.Instance != null && 
                (GameManager.Instance.CurrentState == GameState.Playing || 
                 GameManager.Instance.CurrentState == GameState.Paused))
            {
                GameManager.Instance.TogglePause();
            }
        }
        
        if (GameManager.Instance?.CurrentState == GameState.Playing)
            UpdateProgress();
    }
    
    #endregion
    
    #region Debug Info
    
    void OnGUI()
    {
        if (!showDebugInfo || GameManager.Instance == null) return;
        
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
        GUILayout.Label("=== DEBUG INFO ===");
        GUILayout.Label($"State: {GameManager.Instance.CurrentState}");
        GUILayout.Label($"Level: {GameManager.Instance.CurrentLevelId}");
        GUILayout.Label($"Ducks: {GameManager.Instance.GoodDucksClicked}/{GameManager.Instance.GoodDucksRequired}");
        GUILayout.Label($"Time: {GameManager.Instance.TimeLeft:F1}s");
        GUILayout.Label($"Lives: {GameManager.Instance.Lives}");
        
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
            GUILayout.Label($"Active Ducks: {spawner.ActiveDuckCount}");
        
        GUILayout.EndArea();
    }
    
    #endregion
}