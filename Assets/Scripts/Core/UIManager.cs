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
    [SerializeField] private Button mainMenuButton;
    
    [Header("Pause Panel")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseRestartButton;
    
    [Header("Instructions Panel")]
    [SerializeField] private GameObject instructionsPanel;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private Button startGameButton;
    
    [Header("Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color timerWarningColor = Color.red;
    [SerializeField] private float timerWarningThreshold = 10f;
    
    private Color originalTimerColor;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Store original timer color
        if (timerText != null)
            originalTimerColor = timerText.color;
        
        SetupButtonListeners();
        ValidateUIElements();
    }
    
    void Start()
    {
        Debug.Log("UIManager Start() called");
        
        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreChanged += UpdateScore;
            GameManager.Instance.OnLivesChanged += UpdateLives;
            GameManager.Instance.OnTimeChanged += UpdateTimer;
            GameManager.Instance.OnGameStateChanged += UpdateGameState;
            GameManager.Instance.OnLevelLoaded += UpdateLevelInfo;
            Debug.Log("UIManager subscribed to GameManager events");
        }
        else
        {
            Debug.LogWarning("GameManager.Instance is null in UIManager.Start()");
        }
        
        // Show initial instructions and hide HUD
        Debug.Log("UIManager: Showing initial instructions and hiding HUD");
        HideHUDElements();
        ShowInstructions();
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
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
        // Game Over buttons
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartClicked);
        if (nextLevelButton != null)
            nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        // Pause buttons
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (pauseRestartButton != null)
            pauseRestartButton.onClick.AddListener(OnRestartClicked);
        
        // Instructions button
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
    }
    
    private void ValidateUIElements()
    {
        if (scoreText == null) Debug.LogWarning("Score text not assigned in UIManager");
        if (timerText == null) Debug.LogWarning("Timer text not assigned in UIManager");
        if (livesText == null) Debug.LogWarning("Lives text not assigned in UIManager");
        if (levelText == null) Debug.LogWarning("Level text not assigned in UIManager");
        if (gameOverPanel == null) Debug.LogWarning("Game Over panel not assigned in UIManager");
    }
    
    #endregion
    
    #region HUD Updates
    
    /// <summary>
    /// Update score display
    /// </summary>
    public void UpdateScore(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score:N0}";
        }
    }
    
    /// <summary>
    /// Update timer display with warning colors
    /// </summary>
    public void UpdateTimer(float timeLeft)
    {
        if (timerText != null)
        {
            // Format time as MM:SS
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            // Change coloor when time is low
            if (timeLeft <= timerWarningThreshold)
            {
                timerText.color = Color.Lerp(timerWarningColor, originalTimerColor, timeLeft / timerWarningThreshold);
            }
            else
            {
                timerText.color = originalTimerColor;
            }
        }
    }
    
    /// <summary>
    /// Update lives display
    /// </summary>
    public void UpdateLives(int lives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {lives}";
        }
    }
    
    /// <summary>
    /// Update level information
    /// </summary>
    public void UpdateLevelInfo(LevelData levelData)
    {
        if (levelText != null)
        {
            levelText.text = $"Level: {levelData.levelId}";
        }
        
        UpdateProgress();
    }
    
    /// <summary>
    /// Update progress display
    /// </summary>
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
    
    /// <summary>
    /// Handle game state changes
    /// </summary>
    public void UpdateGameState(GameState newState)
    {
        Debug.Log($"UI: Game state changed to {newState}");
        
        switch (newState)
        {
            case GameState.Menu:
                Debug.Log("UI: Showing instructions");
                ShowInstructions();
                break;
            case GameState.Playing:
                Debug.Log("UI: Showing game HUD - this should enable HUD elements");
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
    
    /// <summary>
    /// Hide all HUD elements during menus
    /// </summary>
    private void HideHUDElements()
    {
        Debug.Log("UIManager: Hiding HUD elements");
        
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (levelText != null) levelText.gameObject.SetActive(false);
        if (progressText != null) progressText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show all HUD elements during gameplay
    /// </summary>
    private void ShowHUDElements()
    {
        Debug.Log("UIManager: Showing HUD elements");
        
        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        if (livesText != null) livesText.gameObject.SetActive(true);
        if (levelText != null) levelText.gameObject.SetActive(true);
        if (progressText != null) progressText.gameObject.SetActive(true);
    }
    
    #endregion
    
    #region Panel Management
    
    /// <summary>
    /// Show instructions panel
    /// </summary>
    private void ShowInstructions()
    {
        Debug.Log("UIManager: ShowInstructions() called");
        Debug.Log($"InstructionsPanel reference: {(instructionsPanel != null ? instructionsPanel.name : "NULL")}");
        
        SetAllPanelsInactive();
        HideHUDElements(); // Hide HUD during instructions
        
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Debug.Log($"UIManager: Instructions panel activated - Active: {instructionsPanel.activeSelf}");
            
            if (instructionsText != null)
            {
                instructionsText.text = 
                    "Welcome to Duck Game!\n\n" +
                    "• Click the GOOD ducks to earn points\n" +
                    "• Avoid clicking DECOY ducks (they cost time)\n" +
                    "• Complete each level before time runs out\n" +
                    "• Small ducks = More points, but harder to click\n" +
                    "• Single life - sudden death!\n" +
                    "• Fail any level = restart from Level 1\n" +
                    "• Click 'Restart' to continue after failing\n\n" +
                    "Good luck!";
                Debug.Log("UIManager: Instructions text updated");
            }
            else
            {
                Debug.LogError("UIManager: Instructions text is null!");
            }
        }
        else
        {
            Debug.LogError("UIManager: Instructions panel is null! Check UIManager assignment in Inspector.");
        }
    }
    
    /// <summary>
    /// Show main game HUD
    /// </summary>
    private void ShowGameHUD()
    {
        Debug.Log("UI: Hiding all panels and showing HUD");
        SetAllPanelsInactive();
        ShowHUDElements(); // Show HUD during gameplay
        UpdateProgress(); // Update progress when game starts
    }
    
    /// <summary>
    /// Show pause panel
    /// </summary>
    private void ShowPausePanel()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
    }
    
    /// <summary>
    /// Show level complete screen
    /// </summary>
    private void ShowLevelComplete()
    {
        HideHUDElements();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            if (gameOverTitle != null)
                gameOverTitle.text = "Level Complete!";

            if (finalScoreText != null && GameManager.Instance != null)
                finalScoreText.text = $"Final Score: {GameManager.Instance.Score:N0}";

            // Show next level button if available and update its label to show next level number
            if (nextLevelButton != null)
            {
                int nextLevel = LevelLoader.Instance?.GetNextLevelId(GameManager.Instance.CurrentLevelId) ?? -1;
                nextLevelButton.gameObject.SetActive(nextLevel > 0);

                // Fix: Use TextMeshProUGUI for button label
                var tmpText = nextLevelButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = nextLevel > 0 ? $"Next Level ({nextLevel})" : "Next Level";
                }
            }
        }
    }
    /// <summary>
    /// Show game over screen
    /// </summary>
    private void ShowGameOver(bool isCompleteGameOver)
    {
        HideHUDElements();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverTitle != null)
            {
                // Single life system - always sudden death
                gameOverTitle.text = "Level Failed!";
            }
            
            if (finalScoreText != null && GameManager.Instance != null)
            {
                // Simple restart message
                finalScoreText.text = "Restart from Level 1";
            }
            
            // Hide next level button for game over
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Show game complete screen
    /// </summary>
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
            
            // Hide next level button
            if (nextLevelButton != null)
                nextLevelButton.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Hide all popup panels
    /// </summary>
    private void SetAllPanelsInactive()
    {
        Debug.Log("UI: Disabling all panels");
        
        if (gameOverPanel != null) 
        {
            gameOverPanel.SetActive(false);
            Debug.Log("UI: Disabled GameOverPanel");
        }
        if (pausePanel != null) 
        {
            pausePanel.SetActive(false);
            Debug.Log("UI: Disabled PausePanel");
        }
        if (instructionsPanel != null) 
        {
            Debug.Log($"UI: InstructionsPanel found: {instructionsPanel.name}, currently active: {instructionsPanel.activeSelf}");
            instructionsPanel.SetActive(false);
            Debug.Log($"UI: After disable - InstructionsPanel active: {instructionsPanel.activeSelf}");
        }
        else
        {
            Debug.LogError("UI: InstructionsPanel is NULL! Check UIManager assignment.");
        }
    }
    
    #endregion
    
    #region Button Handlers
    
    private void OnStartGameClicked()
    {
        Debug.Log("=== OnStartGameClicked called ===");
        
        // Try multiple ways to hide instructions panel
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
            Debug.Log("UI: Manually disabled InstructionsPanel via reference");
        }
        else
        {
            // Find it manually if reference is broken
            GameObject panel = GameObject.Find("InstructionsPanel");
            if (panel != null)
            {
                panel.SetActive(false);
                Debug.Log("UI: Found and disabled InstructionsPanel manually");
            }
            else
            {
                Debug.LogError("UI: Cannot find InstructionsPanel!");
            }
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager.Instance is null!");
            return;
        }
        
        Debug.Log("Calling GameManager.StartGame(true) - coming from menu");
        GameManager.Instance.StartGame(true); // Coming from menu
        Debug.Log("GameManager.StartGame() call completed");
    }
    
    private void OnRestartClicked()
    {
        GameManager.Instance?.RestartLevel();
    }
    
    private void OnNextLevelClicked()
    {
        GameManager.Instance?.AdvanceToNextLevel();
    }
    
    private void OnMainMenuClicked()
    {
        GameManager.Instance?.RestartGame();
    }
    
    private void OnResumeClicked()
    {
        GameManager.Instance?.TogglePause();
    }
    
    #endregion
    
    #region Input Handling
    
    void Update()
    {
        // Handle pause key (ESC) - Using New Input System
        if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
        {
            if (GameManager.Instance != null && 
                (GameManager.Instance.CurrentState == GameState.Playing || 
                 GameManager.Instance.CurrentState == GameState.Paused))
            {
                GameManager.Instance.TogglePause();
            }
        }
        
        // Update progress display during play
        if (GameManager.Instance?.CurrentState == GameState.Playing)
        {
            UpdateProgress();
        }
    }
    
    #endregion
    
    #region Debug Info
    
    void OnGUI()
    {
        if (!showDebugInfo || GameManager.Instance == null) return;
        
        // Show debug info in top-right corner
        GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
        GUILayout.Label("=== DEBUG INFO ===");
        GUILayout.Label($"State: {GameManager.Instance.CurrentState}");
        GUILayout.Label($"Level: {GameManager.Instance.CurrentLevelId}");
        GUILayout.Label($"Ducks: {GameManager.Instance.GoodDucksClicked}/{GameManager.Instance.GoodDucksRequired}");
        GUILayout.Label($"Time: {GameManager.Instance.TimeLeft:F1}s");
        GUILayout.Label($"Lives: {GameManager.Instance.Lives}");
        
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            GUILayout.Label($"Active Ducks: {spawner.ActiveDuckCount}");
        }
        
        GUILayout.EndArea();
    }
    
    #endregion
}