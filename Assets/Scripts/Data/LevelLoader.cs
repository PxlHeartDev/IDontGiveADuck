using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Handles loading and caching level data from JSON files
/// /// </summary>
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }
    
    [Header("Level Settings")]
    [SerializeField] private string levelsPath = "Data/Levels/";
    
    // Cache loaded levels for performance
    private Dictionary<int, LevelData> loadedLevels = new Dictionary<int, LevelData>();
    
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
    /// Loads level data from JSON file or cache
    /// </summary>
    public LevelData LoadLevel(int levelId)
    {
        Debug.Log($"LoadLevel called for level {levelId}");
        
        // Check cache first
        if (loadedLevels.ContainsKey(levelId))
        {
            Debug.Log($"Loading Level {levelId} from cache");
            return loadedLevels[levelId];
        }
        
        // Load from Resources folder
        string fileName = $"level_{levelId:000}";
        Debug.Log($"Loading from file: {fileName}");
        string jsonText = LoadLevelFile(fileName);
        
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogWarning($"Could not load level {levelId}, using default");
            return CreateDefaultLevel(levelId);
        }
        
        try
        {
            Debug.Log($"Parsing JSON for level {levelId}");
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonText);
            levelData.sizeDistribution.Normalize();
            
            // Cache the loaded level
            loadedLevels[levelId] = levelData;
            Debug.Log($"Successfully loaded Level {levelId}: {levelData.levelName}");
            
            // Debug: Log the new fields to verify JSON loading
            levelData.LogLevelData();
            
            return levelData;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing level {levelId}: {e.Message}");
            return CreateDefaultLevel(levelId);
        }
    }
    
    /// <summary>
    /// Loads JSON text from Resources folder
    /// </summary>
    private string LoadLevelFile(string fileName)
    {
        try
        {
            Debug.Log($"Attempting to load level file: {fileName}");
            TextAsset textAsset = Resources.Load<TextAsset>($"{levelsPath}{fileName}");
            
            if (textAsset != null)
            {
                Debug.Log($"Successfully loaded {fileName}, text length: {textAsset.text.Length}");
                return textAsset.text;
            }
            else
            {
                Debug.LogWarning($"TextAsset is null for {fileName} - file not found in Resources/{levelsPath}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load level file {fileName}: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Creates a fallback level configuration
    /// </summary>
    private LevelData CreateDefaultLevel(int levelId)
    {
        return new LevelData
        {
            levelId = levelId,
            levelName = $"Default Level {levelId}",
            goodDucks = 3,
            decoyDucks = 0,
            timeLimit = 30f,
            spawnRate = 3.0f,
            duckLifetime = 5.0f,
            decoyPenalty = 2,
            difficulty = "tutorial",
            backgroundMusic = "tutorial_theme",
            designNotes = "Default level - fallback configuration",
            learningObjective = "Complete the level objectives",
            targetSuccessRate = 0.75f
        };
    }
    
    /// <summary>
    /// Gets next level ID or returns -1 if no more levels
    /// </summary>
    public int GetNextLevelId(int currentLevelId)
    {
        int nextId = currentLevelId + 1;
        
        // Try to load next level to see if it exists
        string fileName = $"level_{nextId:000}";
        TextAsset textAsset = Resources.Load<TextAsset>($"{levelsPath}{fileName}");
        
        return textAsset != null ? nextId : -1;
    }
    
    /// <summary>
    /// Clears level cache (useful for development)
    /// </summary>
    [ContextMenu("Clear Level Cache")]
    public void ClearCache()
    {
        loadedLevels.Clear();
        Debug.Log("Level cache cleared");
    }

    /// <summary>
    /// Debug method to test level progression
    /// </summary>
    [ContextMenu("Test Level Progression")]
    public void TestLevelProgression()
    {
        Debug.Log("=== Testing Level Progression ===");
        
        for (int i = 1; i <= 12; i++)
        {
            LevelData level = LoadLevel(i);
            int nextLevel = GetNextLevelId(i);
            
            Debug.Log($"Level {i}: {level.levelName} -> Next: {(nextLevel > 0 ? nextLevel.ToString() : "END")}");
        }
        
        // Test beyond level 12
        int level13 = GetNextLevelId(12);
        Debug.Log($"Level 13: {(level13 > 0 ? level13.ToString() : "END OF GAME")}");
    }
}