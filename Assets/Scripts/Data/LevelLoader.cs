using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Handles loading and caching level data from JSON files
/// Save this as: Assets/Scripts/Data/LevelLoader.cs
/// </summary>
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
        // Check cache first
        if (loadedLevels.ContainsKey(levelId))
        {
            Debug.Log($"Loading Level {levelId} from cache");
            return loadedLevels[levelId];
        }
        
        // Load from Resources folder
        string fileName = $"level_{levelId:000}";
        string jsonText = LoadLevelFile(fileName);
        
        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogWarning($"Could not load level {levelId}, using default");
            return CreateDefaultLevel(levelId);
        }
        
        try
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonText);
            levelData.sizeDistribution.Normalize();
            
            // Cache the loaded level
            loadedLevels[levelId] = levelData;
            Debug.Log($"Successfully loaded Level {levelId}: {levelData.levelName}");
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
            TextAsset textAsset = Resources.Load<TextAsset>($"{levelsPath}{fileName}");
            return textAsset?.text;
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
            difficulty = "tutorial"
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
}