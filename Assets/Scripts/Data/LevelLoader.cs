using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles loading level data from JSON files in Resources/Data/Levels
/// </summary>
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    
    private Dictionary<int, LevelData> levelCache = new Dictionary<int, LevelData>();
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Load a specific level by ID
    /// </summary>
    public LevelData LoadLevel(int levelId)
    {
        // Check cache first
        if (levelCache.ContainsKey(levelId))
        {
            return levelCache[levelId];
        }
        
        // Load from Resources
        string fileName = $"level_{levelId:D3}"; // Format as "level_001", "level_002", etc.
        TextAsset levelFile = Resources.Load<TextAsset>($"Data/Levels/{fileName}");
        
        if (levelFile != null)
        {
            try
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(levelFile.text);
                levelCache[levelId] = levelData;
                return levelData;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to parse level {levelId}: {e.Message}");
                return CreateDefaultLevel(levelId);
            }
        }
        else
        {
            Debug.LogWarning($"Level file not found: {fileName}");
            return CreateDefaultLevel(levelId);
        }
    }
    
    /// <summary>
    /// Get the next level ID, or -1 if no more levels
    /// </summary>
    public int GetNextLevelId(int currentLevelId)
    {
        int nextLevelId = currentLevelId + 1;
        string fileName = $"level_{nextLevelId:D3}";
        TextAsset levelFile = Resources.Load<TextAsset>($"Data/Levels/{fileName}");
        
        return levelFile != null ? nextLevelId : -1;
    }
    
    /// <summary>
    /// Create a default level if loading fails
    /// </summary>
    private LevelData CreateDefaultLevel(int levelId)
    {
        LevelData defaultLevel = new LevelData
        {
            levelId = levelId,
            levelName = $"Default Level {levelId}",
            goodDucks = 3,
            decoyDucks = 1,
            timeLimit = 30f,
            spawnRate = 3.0f,
            duckLifetime = 5.0f,
            decoyPenalty = 3,
            sizeDistribution = new LevelData.SizeDistribution
            {
                large = 0.6f,
                medium = 0.3f,
                small = 0.1f
            },
            specialMechanics = new string[0],
            backgroundMusic = "tutorial_theme",
            difficulty = "normal",
            designNotes = "Default level created due to missing level file",
            targetSuccessRate = 0.8f,
            learningObjective = "Complete the level",
            powerUpsAvailable = false
        };
        
        return defaultLevel;
    }
    
    /// <summary>
    /// Clear the level cache
    /// </summary>
    public void ClearCache()
    {
        levelCache.Clear();
    }
}
