using UnityEngine;

/// <summary>
/// Data structure for level configuration loaded from JSON files
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
    
    [Header("Game Features")]
    public string[] specialMechanics;
    public string backgroundMusic;
    public string difficulty;
    
    [Header("Design & Learning")]
    public string designNotes;
    public float targetSuccessRate;
    public string learningObjective;
    
    // Default constructor for JSON deserialisation
    public LevelData()
    {
        sizeDistribution = new SizeDistribution();
        specialMechanics = new string[0];
        designNotes = "";
        learningObjective = "";
        targetSuccessRate = 0.75f; // Default 75% success rate
    }
    
    /// <summary>
    /// Debug method to log level data for testing
    /// </summary>
    public void LogLevelData()
    {
        Debug.Log($"=== Level {levelId}: {levelName} ===");
        Debug.Log($"Difficulty: {difficulty}");
        Debug.Log($"Background Music: {backgroundMusic}");
        Debug.Log($"Target Success Rate: {targetSuccessRate:P0}");
        Debug.Log($"Design Notes: {designNotes}");
        Debug.Log($"Learning Objective: {learningObjective}");
        Debug.Log($"Special Mechanics: {string.Join(", ", specialMechanics)}");
        Debug.Log($"Ducks: {goodDucks} good, {decoyDucks} decoys");
        Debug.Log($"Time: {timeLimit}s, Spawn Rate: {spawnRate}s, Lifetime: {duckLifetime}s");
        Debug.Log($"Decoy Penalty: {decoyPenalty}s");
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