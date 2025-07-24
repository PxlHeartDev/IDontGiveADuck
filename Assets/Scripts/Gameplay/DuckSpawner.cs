using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles spawning of ducks based on level configuration
/// Save this as: Assets/Scripts/Gameplay/DuckSpawner.cs
/// </summary>
public class DuckSpawner : MonoBehaviour
{
    [Header("Duck Prefabs")]
    [SerializeField] private GameObject[] goodDuckPrefabs; // [0]=Large, [1]=Medium, [2]=Small
    [SerializeField] private GameObject[] decoyDuckPrefabs; // [0]=Large, [1]=Medium, [2]=Small
    
    [Header("Spawn Area")]
    [SerializeField] private BoxCollider2D spawnArea;
    [SerializeField] private float spawnPadding = 1f; // Distance from spawn area edges
    
    [Header("Debug Settings")]
    [SerializeField] private bool showSpawnArea = true;
    [SerializeField] private Color spawnAreaColor = Color.green;
    
    // Current level configuration
    private LevelData currentLevel;
    private bool isSpawning = false;
    
    // Spawn tracking
    private int goodDucksRemaining = 0;
    private int decoyDucksRemaining = 0;
    private List<GameObject> activeDucks = new List<GameObject>();
    
    // Coroutine references
    private Coroutine spawnCoroutine;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // If no spawn area assigned, create one
        if (spawnArea == null)
        {
            SetupDefaultSpawnArea();
        }
        
        ValidatePrefabs();
    }
    
    void OnDrawGizmos()
    {
        if (showSpawnArea && spawnArea != null)
        {
            Gizmos.color = spawnAreaColor;
            Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
        }
    }
    
    #endregion
    
    #region Setup and Validation
    
    private void SetupDefaultSpawnArea()
    {
        // Create a default spawn area covering most of the screen
        GameObject spawnAreaObj = new GameObject("SpawnArea");
        spawnAreaObj.transform.SetParent(transform);
        
        spawnArea = spawnAreaObj.AddComponent<BoxCollider2D>();
        spawnArea.isTrigger = true;
        
        // Set size based on camera bounds
        Camera cam = Camera.main;
        if (cam != null)
        {
            float height = 2f * cam.orthographicSize;
            float width = height * cam.aspect;
            
            spawnArea.size = new Vector2(width * 0.8f, height * 0.8f);
        }
        else
        {
            spawnArea.size = new Vector2(10f, 6f);
        }
        
        Debug.Log("Created default spawn area");
    }
    
    private void ValidatePrefabs()
    {
        // Check good duck prefabs
        if (goodDuckPrefabs == null || goodDuckPrefabs.Length < 3)
        {
            Debug.LogError("DuckSpawner: Good duck prefabs array must have 3 elements [Large, Medium, Small]");
        }
        
        // Check decoy duck prefabs
        if (decoyDuckPrefabs == null || decoyDuckPrefabs.Length < 3)
        {
            Debug.LogError("DuckSpawner: Decoy duck prefabs array must have 3 elements [Large, Medium, Small]");
        }
        
        // Validate prefab components
        for (int i = 0; i < goodDuckPrefabs.Length; i++)
        {
            if (goodDuckPrefabs[i] != null && goodDuckPrefabs[i].GetComponent<GoodDuck>() == null)
            {
                Debug.LogError($"Good duck prefab {i} is missing GoodDuck component");
            }
        }
        
        for (int i = 0; i < decoyDuckPrefabs.Length; i++)
        {
            if (decoyDuckPrefabs[i] != null && decoyDuckPrefabs[i].GetComponent<DecoyDuck>() == null)
            {
                Debug.LogError($"Decoy duck prefab {i} is missing DecoyDuck component");
            }
        }
    }
    
    #endregion
    
    #region Public Interface
    
    /// <summary>
    /// Start spawning ducks for the given level
    /// </summary>
    public void StartSpawning(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("Cannot start spawning - level data is null");
            return;
        }
        
        currentLevel = levelData;
        goodDucksRemaining = levelData.goodDucks;
        decoyDucksRemaining = levelData.decoyDucks;
        
        isSpawning = true;
        
        // Clear any existing ducks
        ClearActiveDucks();
        
        // Start spawning coroutine
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        
        spawnCoroutine = StartCoroutine(SpawnDucksCoroutine());
        
        Debug.Log($"Started spawning for level {levelData.levelId}: {levelData.goodDucks} good, {levelData.decoyDucks} decoys");
    }
    
    /// <summary>
    /// Stop spawning ducks
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        Debug.Log("Duck spawning stopped");
    }
    
    /// <summary>
    /// Clear all active ducks from the scene
    /// </summary>
    public void ClearActiveDucks()
    {
        foreach (GameObject duck in activeDucks)
        {
            if (duck != null)
            {
                Destroy(duck);
            }
        }
        
        activeDucks.Clear();
        Debug.Log("All active ducks cleared");
    }
    
    #endregion
    
    #region Spawning Logic
    
    /// <summary>
    /// Main spawning coroutine
    /// </summary>
    private IEnumerator SpawnDucksCoroutine()
    {
        while (isSpawning && (goodDucksRemaining > 0 || decoyDucksRemaining > 0))
        {
            // Wait for spawn interval
            yield return new WaitForSeconds(currentLevel.spawnRate);
            
            // Decide what type of duck to spawn
            bool spawnGoodDuck = ShouldSpawnGoodDuck();
            
            if (spawnGoodDuck && goodDucksRemaining > 0)
            {
                SpawnGoodDuck();
            }
            else if (!spawnGoodDuck && decoyDucksRemaining > 0)
            {
                SpawnDecoyDuck();
            }
            else if (goodDucksRemaining > 0)
            {
                // Force spawn good duck if no decoys left
                SpawnGoodDuck();
            }
            else if (decoyDucksRemaining > 0)
            {
                // Force spawn decoy if no good ducks left
                SpawnDecoyDuck();
            }
        }
        
        Debug.Log("Spawning completed - all ducks spawned");
    }
    
    /// <summary>
    /// Determine whether to spawn a good duck or decoy
    /// </summary>
    private bool ShouldSpawnGoodDuck()
    {
        if (goodDucksRemaining <= 0) return false;
        if (decoyDucksRemaining <= 0) return true;
        
        // Calculate spawn probability based on remaining counts
        float totalRemaining = goodDucksRemaining + decoyDucksRemaining;
        float goodDuckProbability = goodDucksRemaining / totalRemaining;
        
        // Add some randomness to avoid predictable patterns
        goodDuckProbability += Random.Range(-0.1f, 0.1f);
        goodDuckProbability = Mathf.Clamp01(goodDuckProbability);
        
        return Random.value < goodDuckProbability;
    }
    
    /// <summary>
    /// Spawn a good duck
    /// </summary>
    private void SpawnGoodDuck()
    {
        GameObject prefab = SelectGoodDuckPrefab();
        if (prefab == null) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Configure duck with level-specific properties
        GoodDuck goodDuck = duck.GetComponent<GoodDuck>();
        if (goodDuck != null)
        {
            goodDuck.Initialize(currentLevel.duckLifetime);
        }
        
        // Track active duck
        activeDucks.Add(duck);
        goodDucksRemaining--;
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
        }
        
        Debug.Log($"Spawned good duck. Remaining: {goodDucksRemaining}");
    }
    
    /// <summary>
    /// Spawn a decoy duck
    /// </summary>
    private void SpawnDecoyDuck()
    {
        GameObject prefab = SelectDecoyDuckPrefab();
        if (prefab == null) return;
        
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject duck = Instantiate(prefab, spawnPosition, Quaternion.identity);
        
        // Configure duck with level-specific properties
        DecoyDuck decoyDuck = duck.GetComponent<DecoyDuck>();
        if (decoyDuck != null)
        {
            decoyDuck.Initialize(currentLevel.duckLifetime);
        }
        
        // Track active duck
        activeDucks.Add(duck);
        decoyDucksRemaining--;
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDuckSpawned();
        }
        
        Debug.Log($"Spawned decoy duck. Remaining: {decoyDucksRemaining}");
    }
    
    #endregion
    
    #region Prefab Selection
    
    /// <summary>
    /// Select a good duck prefab based on size distribution
    /// </summary>
    private GameObject SelectGoodDuckPrefab()
    {
        if (goodDuckPrefabs == null || goodDuckPrefabs.Length < 3)
        {
            Debug.LogError("Good duck prefabs not properly configured");
            return null;
        }
        
        float rand = Random.value;
        SizeDistribution dist = currentLevel.sizeDistribution;
        
        if (rand < dist.large)
            return goodDuckPrefabs[0]; // Large
        else if (rand < dist.large + dist.medium)
            return goodDuckPrefabs[1]; // Medium
        else
            return goodDuckPrefabs[2]; // Small
    }
    
    /// <summary>
    /// Select a decoy duck prefab based on size distribution
    /// </summary>
    private GameObject SelectDecoyDuckPrefab()
    {
        if (decoyDuckPrefabs == null || decoyDuckPrefabs.Length < 3)
        {
            Debug.LogError("Decoy duck prefabs not properly configured");
            return null;
        }
        
        float rand = Random.value;
        SizeDistribution dist = currentLevel.sizeDistribution;
        
        if (rand < dist.large)
            return decoyDuckPrefabs[0]; // Large
        else if (rand < dist.large + dist.medium)
            return decoyDuckPrefabs[1]; // Medium
        else
            return decoyDuckPrefabs[2]; // Small
    }
    
    #endregion
    
    #region Spawn Position
    
    /// <summary>
    /// Get a random position within the spawn area
    /// </summary>
    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnArea == null)
        {
            Debug.LogWarning("No spawn area defined, using origin");
            return Vector3.zero;
        }
        
        Bounds bounds = spawnArea.bounds;
        
        float x = Random.Range(
            bounds.min.x + spawnPadding,
            bounds.max.x - spawnPadding
        );
        
        float y = Random.Range(
            bounds.min.y + spawnPadding,
            bounds.max.y - spawnPadding
        );
        
        return new Vector3(x, y, 0);
    }
    
    #endregion
    
    #region Cleanup
    
    void OnDestroy()
    {
        StopSpawning();
        ClearActiveDucks();
    }
    
    #endregion
    
    #region Public Getters (for UI/debugging)
    
    public bool IsSpawning => isSpawning;
    public int GoodDucksRemaining => goodDucksRemaining;
    public int DecoyDucksRemaining => decoyDucksRemaining;
    public int ActiveDuckCount => activeDucks.Count;
    public LevelData CurrentLevel => currentLevel;
    
    #endregion
}