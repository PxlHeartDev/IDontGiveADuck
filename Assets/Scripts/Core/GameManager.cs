using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Central game management system - handles game state, scoring, level progression
/// Save this as: Assets/Scripts/Core/GameManager.cs
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Configuration")]
    [SerializeField] private int startingLives = 1; // Single life - sudden death
    [SerializeField] private int currentLevelId = 1;
    
    [Header("Current Game State")]
    [SerializeField] private int score = 0;
    [SerializeField] private int lives = 1; // Single life
    [SerializeField] private float timeLeft = 30f;
    [SerializeField] private int goodDucksClicked = 0;
    [SerializeField] private int goodDucksMissed = 0;
    
    // Level configuration
    private LevelData currentLevel;
    private GameState currentState = GameState.Menu;
    
    // Game statistics
    private float levelStartTime;
    private int totalDucksSpawned = 0;
    
    // Events for UI updates
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnLivesChanged;
    public System.Action<float> OnTimeChanged;
    public System.Action<GameState> OnGameStateChanged;
    public System.Action<LevelData> OnLevelLoaded;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadCurrentLevel();
    }
    
    void Update()
    {
        if (currentState == GameState.Playing)
        {
            UpdateGameTimer();
        }
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeGame()
    {
        lives = startingLives;
        score = 0;
        currentState = GameState.Menu;
        
        Debug.Log("GameManager initialized");
    }
    
    #endregion
    
    #region Level Management
    
    /// <summary>
    /// Load and configure the current level
    /// </summary>
    private void LoadCurrentLevel()
    {
        if (LevelLoader.Instance == null)
        {
            Debug.LogError("LevelLoader not found! Make sure LevelLoader is in the scene.");
            return;
        }
        
        currentLevel = LevelLoader.Instance.LoadLevel(currentLevelId);
        
        if (currentLevel == null)
        {
            Debug.LogError($"Failed to load level {currentLevelId}");
            return;
        }
        
        // Reset level-specific values
        timeLeft = currentLevel.timeLimit;
        goodDucksClicked = 0;
        goodDucksMissed = 0;
        totalDucksSpawned = 0;
        
        // Notify systems about level load
        OnLevelLoaded?.Invoke(currentLevel);
        
        Debug.Log($"Level {currentLevel.levelId} loaded: {currentLevel.levelName}");
        Debug.Log($"Target: {currentLevel.goodDucks} good ducks, {currentLevel.decoyDucks} decoys");
    }
    
    /// <summary>
    /// Advance to the next level
    /// </summary>
    public void AdvanceToNextLevel()
    {
        int nextLevelId = LevelLoader.Instance.GetNextLevelId(currentLevelId);
        
        if (nextLevelId > 0)
        {
            currentLevelId = nextLevelId;
            LoadCurrentLevel();
            StartGame(false); // Not from menu - LoadCurrentLevel already triggered OnLevelLoaded
        }
        else
        {
            // No more levels - game complete!
            CompleteGame();
        }
    }
    
    /// <summary>
    /// Jump directly to a specific level (for testing)
    /// </summary>
    public void JumpToLevel(int levelId)
    {
        Debug.Log($"GameManager.JumpToLevel({levelId}) called");
        
        // Stop any existing spawner first
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        // Set the target level
        currentLevelId = levelId;
        
        // Load the level
        LoadCurrentLevel();
        
        // Start the game immediately
        StartGame(false);
        
        Debug.Log($"Jumped to level {levelId}: {currentLevel?.levelName}");
    }
    
    /// <summary>
    /// Restart from the latest checkpoint or current level if no checkpoint
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("=== RestartLevel called - Complete game reset ===");
        
        // Stop any existing spawner first
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            Debug.Log("Stopping existing spawner");
            spawner.StopSpawning();
        }
        else
        {
            Debug.LogWarning("No DuckSpawner found during restart");
        }
        
        // Complete game reset - always restart from level 1
        Debug.Log("Restarting from level 1 (complete reset)");
        currentLevelId = 1;
        score = 0;
        lives = 1; // Reset to single life
        
        // Reset all game state
        timeLeft = 30f;
        goodDucksClicked = 0;
        goodDucksMissed = 0;
        totalDucksSpawned = 0;
        levelStartTime = 0f;
        
        // Update UI
        OnLivesChanged?.Invoke(lives);
        OnScoreChanged?.Invoke(score);
        OnTimeChanged?.Invoke(timeLeft);
        
        // Load the level and show menu (don't auto-start)
        LoadCurrentLevel();
        currentState = GameState.Menu;
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log("Restart complete - showing menu with complete reset");
    }
    
    #endregion
    
    #region Game Flow Control
    
    /// <summary>
    /// Start the current level
    /// </summary>
    public void StartGame(bool fromMenu = false)
    {
        Debug.Log($"GameManager.StartGame() called (fromMenu: {fromMenu})");
        
        if (currentLevel == null)
        {
            Debug.LogError("Cannot start game - no level loaded!");
            return;
        }
        
        Debug.Log($"Starting game with level: {currentLevel.levelName}");
        Debug.Log($"Level config - Good ducks: {currentLevel.goodDucks}, Decoys: {currentLevel.decoyDucks}, Time: {currentLevel.timeLimit}s");
        
        // Change state to Playing
        currentState = GameState.Playing;
        
        // Only trigger OnLevelLoaded if we're coming from Menu state
        // (when advancing levels, LoadCurrentLevel already triggered OnLevelLoaded)
        if (fromMenu)
        {
            Debug.Log("StartGame: Triggering OnLevelLoaded for music transition from menu");
            OnLevelLoaded?.Invoke(currentLevel);
        }
        
        levelStartTime = Time.time;
        
        // Configure spawner
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            Debug.Log($"Found DuckSpawner: {spawner.name}, starting spawning");
            spawner.StartSpawning(currentLevel);
            Debug.Log("DuckSpawner.StartSpawning() called");
        }
        else
        {
            Debug.LogError("DuckSpawner not found! Make sure DuckSpawner is in the scene.");
        }
        
        // Notify UI
        OnGameStateChanged?.Invoke(currentState);
        
        Debug.Log($"Game started - Level {currentLevel.levelId}");
    }
    
    /// <summary>
    /// End the current game/level
    /// </summary>
    public void EndGame(bool won)
    {
        currentState = won ? GameState.LevelComplete : GameState.GameOver;
        
        // Stop spawner
        DuckSpawner spawner = FindFirstObjectByType<DuckSpawner>();
        if (spawner != null)
        {
            spawner.StopSpawning();
        }
        
        // Calculate final stats
        float levelTime = Time.time - levelStartTime;
        float accuracy = totalDucksSpawned > 0 ? (float)goodDucksClicked / totalDucksSpawned : 0f;
        
        Debug.Log($"Level {(won ? "WON" : "LOST")} in {levelTime:F1}s - Accuracy: {accuracy:P1}");
        
        // Notify UI
        OnGameStateChanged?.Invoke(currentState);
        
        // Handle level completion or game over
        if (won)
        {
            HandleLevelComplete();
        }
        else
        {
            HandleGameOver();
        }
    }
    
    /// <summary>
    /// Pause/unpause the game
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Playing)
        {
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (currentState == GameState.Paused)
        {
            currentState = GameState.Playing;
            Time.timeScale = 1f;
        }
        
        OnGameStateChanged?.Invoke(currentState);
    }
    
    #endregion
    
    #region Duck Event Handlers
    
    /// <summary>
    /// Called when player clicks a good duck
    /// </summary>
    public void OnGoodDuckClicked(GoodDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        score += duck.PointValue;
        goodDucksClicked++;
        
        OnScoreChanged?.Invoke(score);
        
        Debug.Log($"Good duck clicked! Score: {score}, Progress: {goodDucksClicked}/{currentLevel.goodDucks}");
        
        // Check win condition
        if (goodDucksClicked >= currentLevel.goodDucks)
        {
            EndGame(true);
        }
    }
    
    /// <summary>
    /// Called when a good duck expires (player missed it)
    /// </summary>
    public void OnGoodDuckMissed(GoodDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        goodDucksMissed++;
        
        Debug.Log($"Good duck missed! Total missed: {goodDucksMissed}");
        
        // Could implement penalty for missing ducks here
        // For now, just track the statistic
    }
    
    /// <summary>
    /// Called when player clicks a decoy duck
    /// </summary>
    public void OnDecoyDuckClicked(DecoyDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        // Apply time penalty from current level config
        timeLeft -= currentLevel.decoyPenalty;
        
        OnTimeChanged?.Invoke(timeLeft);
        
        Debug.Log($"Decoy clicked! Time penalty: -{currentLevel.decoyPenalty}s, Time left: {timeLeft:F1}s");
        
        // Check if time penalty caused game over
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame(false);
        }
    }
    
    /// <summary>
    /// Called when a decoy duck expires naturally
    /// </summary>
    public void OnDecoyDuckExpired(DecoyDuck duck)
    {
        if (currentState != GameState.Playing) return;
        
        Debug.Log("Decoy duck expired naturally - no penalty");
        // No penalty for decoys that expire naturally
    }
    
    /// <summary>
    /// Called when spawner creates a new duck
    /// </summary>
    public void OnDuckSpawned()
    {
        totalDucksSpawned++;
    }
    
    #endregion
    
    #region Game Timer
    
    private void UpdateGameTimer()
    {
        timeLeft -= Time.deltaTime;
        OnTimeChanged?.Invoke(timeLeft);
        
        // Check time-based game over
        if (timeLeft <= 0)
        {
            timeLeft = 0;
            EndGame(false);
        }
    }
    
    #endregion
    
    #region Game Completion Handlers
    
    private void HandleLevelComplete()
    {
        Debug.Log($"Level {currentLevel.levelId} complete!");
        
        // Award bonus points for remaining time
        int timeBonus = Mathf.RoundToInt(timeLeft * 10);
        score += timeBonus;
        OnScoreChanged?.Invoke(score);
        
        Debug.Log($"Time bonus: {timeBonus} points");
    }
    
    private void HandleGameOver()
    {
        Debug.Log($"Game Over! Single life lost - sudden death!");
        
        // Don't automatically restart - show game over screen instead
        // The restart logic is handled in RestartLevel() when player clicks restart button
        Debug.Log("Showing game over screen - player must click restart to continue");
    }
    
    private void CompleteGame()
    {
        currentState = GameState.GameComplete;
        OnGameStateChanged?.Invoke(currentState);
        
        Debug.Log("Congratulations! All levels completed!");
    }
    
    #endregion
    
    #region Public Getters
    
    public int Score => score;
    public int Lives => lives;
    public float TimeLeft => timeLeft;
    public GameState CurrentState => currentState;
    public LevelData CurrentLevel => currentLevel;
    public int CurrentLevelId => currentLevelId;
    public int GoodDucksClicked => goodDucksClicked;
    public int GoodDucksRequired => currentLevel?.goodDucks ?? 0;
    public float LevelProgress => currentLevel != null ? (float)goodDucksClicked / currentLevel.goodDucks : 0f;
    
    #endregion
    
    #region Scene Management
    
    /// <summary>
    /// Restart the entire game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
    
    #endregion

    /// <summary>
    /// Debug method to test level loading and audio integration
    /// </summary>
    [ContextMenu("Test Level Loading")]
    public void TestLevelLoading()
    {
        Debug.Log("=== Testing Level Loading ===");
        
        // Test loading each level and verify OnLevelLoaded fires
        for (int i = 1; i <= 12; i++)
        {
            Debug.Log($"Testing level {i}...");
            
            // Temporarily change current level
            int originalLevel = currentLevelId;
            currentLevelId = i;
            
            // Load the level (this should trigger OnLevelLoaded)
            LoadCurrentLevel();
            
            // Verify level data loaded correctly
            if (currentLevel != null)
            {
                Debug.Log($"✅ Level {i} loaded: {currentLevel.levelName}");
                Debug.Log($"   Background Music: {currentLevel.backgroundMusic}");
                Debug.Log($"   Difficulty: {currentLevel.difficulty}");
                Debug.Log($"   Special Mechanics: {string.Join(", ", currentLevel.specialMechanics)}");
            }
            else
            {
                Debug.LogError($"❌ Failed to load level {i}");
            }
            
            // Restore original level
            currentLevelId = originalLevel;
        }
        
        // Restore original level data
        LoadCurrentLevel();
    }
}

/// <summary>
/// Game state enumeration
/// </summary>
public enum GameState
{
    Menu,
    Playing,
    Paused,
    LevelComplete,
    GameOver,
    GameComplete
}