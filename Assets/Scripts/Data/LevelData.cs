using UnityEngine;

/// <summary>
/// Data structure for level configuration loaded from JSON files
/// Save this as: Assets/Scripts/Data/LevelData.cs
/// </summary>
[System.Serializable]
public class LevelData
{
    [Header("Level Identity")]
    public int levelId;
    public string levelName;
    
    [Header("Duck Configuration")]
    public int goodDucks;
    public int decoyDucks;
    public float timeLimit = 30f;
    public float spawnRate;
    public float duckLifetime;
    public int decoyPenalty;
    
    [Header("Size Distribution")]
    public SizeDistribution sizeDistribution;
    
    [Header("Special Features")]
    public string[] specialMechanics;
    public string backgroundMusic;
    public string difficulty;
    
    // Default constructor for JSON deserialization
    public LevelData()
    {
        sizeDistribution = new SizeDistribution();
        specialMechanics = new string[0];
    }
}

[System.Serializable]
public class SizeDistribution
{
    [Range(0f, 1f)] public float large = 0.6f;
    [Range(0f, 1f)] public float medium = 0.35f;
    [Range(0f, 1f)] public float small = 0.05f;
    
    /// <summary>
    /// Ensures distribution values add up to 1.0
    /// </summary>
    public void Normalize()
    {
        float total = large + medium + small;
        if (total > 0)
        {
            large /= total;
            medium /= total;
            small /= total;
        }
        else
        {
            // Default fallback
            large = 0.6f;
            medium = 0.35f;
            small = 0.05f;
        }
    }
}