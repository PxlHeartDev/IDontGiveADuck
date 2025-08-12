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
    [SerializeField] private int startingLives = 3;
    [SerializeField] private int currentLevelId = 1;
    [SerializeField] private int checkpointLevel = 6; // Checkpoint at level 6 (halfway through 12 levels)
    
    [Header("Current Game State")]
    [SerializeField] private int score = 0;
    [SerializeField] private int lives = 3;
    [SerializeField] private float timeLeft = 30f;
    [SerializeField] private int goodDucksClicked = 0;
    [SerializeField] private int goodDucksMissed = 0;
    
    // Checkpoint system
    private CheckpointData checkpointData;
    private bool hasCheckpoint = false;
    
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
    
    #region Checkpoint Data Structure
    
    [System.Serializable]
    public class CheckpointData
    {
        public int checkpointLevel;
        public int savedScore;
        public int savedLives;
        public int currentLevel;
        
        public CheckpointData(int level, int score, int lives, int current)
        {
            checkpointLevel = level;
            savedScore = score;
            savedLives = lives;
            currentLevel = current;
        }
    }
    
    #endregion
    
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
        
        // Load checkpoint data if it exists
        LoadCheckpointData();
        
        Debug.Log("GameManager initialized");
    }
    
    #endregion
    
    #region Checkpoint System
    
    /// <summary>
    /// Save checkpoint data to PlayerPrefs
    /// </summary>
    private void SaveCheckpointData()
    {
        if (checkpointData != null)
        {
            string json = JsonUtility.ToJson(checkpointData);
            PlayerPrefs.SetString("CheckpointData", json);
            PlayerPrefs.Save();
            Debug.Log($"Checkpoint saved at level {checkpointData.checkpointLevel}");
        }
    }
    
    /// <summary>
    /// Load checkpoint data from PlayerPrefs
    /// </summary>
    private void LoadCheckpointData()
    {
        if (PlayerPrefs.HasKey("CheckpointData"))
        {
            string json = PlayerPrefs.GetString("CheckpointData");
            checkpointData = JsonUtility.FromJson<CheckpointData>(json);
            hasCheckpoint = true;
            Debug.Log($"Checkpoint loaded from level {checkpointData.checkpointLevel}");
        }
        else
        {
            hasCheckpoint = false;
            Debug.Log("No checkpoint data found");
        }
    }
    
    /// <summary>
    /// Clear checkpoint data (for complete game restart)
    /// </summary>
    private void ClearCheckpointData()
    {
        PlayerPrefs.DeleteKey("CheckpointData");
        checkpointData = null;
        hasCheckpoint = false;
        Debug.Log("Checkpoint data cleared");
    }
    
    /// <summary>
    /// Check if current level is a checkpoint level
    /// </summary>
    private bool IsCheckpointLevel(int levelId)
    {
        return levelId == checkpointLevel;
    }
    
    /// <summary>
    /// Save checkpoint when reaching checkpoint level
    /// </summary>
    private void SaveCheckpoint()
    {
        checkpointData = new CheckpointData(
            currentLevelId,
            score,
            lives,
            currentLevelId
        );
        hasCheckpoint = true;
        SaveCheckpointData();
        Debug.Log($"Checkpoint saved at level {currentLevelId} with score {score} and lives {lives}");
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
        // Save checkpoint if this was a checkpoint level
        if (IsCheckpointLevel(currentLevelId))
        {
            SaveCheckpoint();
        }
        
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
    /// Restart from the latest checkpoint or current level if no checkpoint
    /// </summary>
    public void RestartLevel()
    {
        if (hasCheckpoint && checkpointData != null)
        {
            // Restart from checkpoint
            currentLevelId = checkpointData.checkpointLevel;
            score = checkpointData.savedScore;
            lives = checkpointData.savedLives;
            
            Debug.Log($"Restarting from checkpoint: Level {currentLevelId}, Score {score}, Lives {lives}");
        }
        else
        {
            // No checkpoint, restart from current level with current score
            Debug.Log($"No checkpoint found, restarting from current level {currentLevelId}");
        }
        
        LoadCurrentLevel();
        StartGame(false); // Not from menu - LoadCurrentLevel already triggered OnLevelLoaded
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
            Debug.Log("Found DuckSpawner, starting spawning");
            spawner.StartSpawning(currentLevel);
        }
        else
        {
            Debug.LogError("DuckSpawner not found!");
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
        lives--;
        OnLivesChanged?.Invoke(lives);
        
        Debug.Log($"Game Over! Lives remaining: {lives}");
        
        if (lives <= 0)
        {
            // Complete game over - reset everything and clear checkpoint
            Debug.Log("All lives lost - complete game over!");
            lives = startingLives; // Reset lives for next game
            OnLivesChanged?.Invoke(lives); // Update UI
            ClearCheckpointData(); // Clear checkpoint data
            currentState = GameState.CompleteGameOver;
            OnGameStateChanged?.Invoke(currentState);
        }
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
    public bool HasCheckpoint => hasCheckpoint;
    public int CheckpointLevel => checkpointData?.checkpointLevel ?? -1;
    
    #endregion
    
    #region Scene Management
    
    /// <summary>
    /// Restart the entire game
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        ClearCheckpointData(); // Clear checkpoint for fresh start
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
    /// Debug method to test checkpoint system
    /// </summary>
    [ContextMenu("Test Checkpoint System")]
    public void TestCheckpointSystem()
    {
        Debug.Log("=== Testing Checkpoint System ===");
        Debug.Log($"Checkpoint Level: {checkpointLevel}");
        Debug.Log($"Current Level: {currentLevelId}");
        Debug.Log($"Has Checkpoint: {hasCheckpoint}");
        
        if (checkpointData != null)
        {
            Debug.Log($"Checkpoint Data - Level: {checkpointData.checkpointLevel}, Score: {checkpointData.savedScore}, Lives: {checkpointData.savedLives}");
        }
        
        // Test checkpoint detection for various levels
        for (int i = 1; i <= 12; i++)
        {
            bool isCheckpoint = IsCheckpointLevel(i);
            Debug.Log($"Level {i}: {(isCheckpoint ? "CHECKPOINT" : "Normal")}");
        }
    }
    
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
    CompleteGameOver,
    GameComplete
}