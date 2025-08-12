using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Centralised audio management system for all game sounds
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameOverMusic;
    [SerializeField] private AudioClip victoryMusic;
    
    [Header("Level-Specific Music")]
    [SerializeField] private AudioClip tutorialTheme;
    [SerializeField] private AudioClip actionTheme;
    [SerializeField] private AudioClip challengeTheme;
    [SerializeField] private AudioClip bossTheme;
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip levelStartSound;
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip gameOverSound;
    
    [Header("Duck Sounds")]
    [SerializeField] private AudioClip duckClickDecoySound;
    [SerializeField] private AudioClip duckClickGoodSound;
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.6f;
    
    [Header("Audio Settings")]
    [SerializeField] private bool enableMusic = true;
    [SerializeField] private bool enableSFX = true;
    [SerializeField] private bool enableUI = true;
    [SerializeField] private float musicFadeSpeed = 1f;
    
    // Audio pools for overlapping sounds
    private Queue<AudioSource> sfxPool = new Queue<AudioSource>();
    private List<AudioSource> activeSfxSources = new List<AudioSource>();
    
    // Current music tracking
    private AudioClip currentMusic;
    private Coroutine musicFadeCoroutine;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Subscribe to game events for automatic audio
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnLevelLoaded += OnLevelLoaded;
        }
        
        // Start with menu music
        PlayMusic(menuMusic);
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnLevelLoaded -= OnLevelLoaded;
        }
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeAudioManager()
    {
        Debug.Log("InitializeAudioManager called");
        
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            Debug.Log("Creating music source...");
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            Debug.Log($"Music source created: {musicSource != null}");
        }
        else
        {
            Debug.Log($"Music source already exists: {musicSource.name}");
        }
        
        if (sfxSource == null)
        {
            Debug.Log("Creating SFX source...");
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            Debug.Log($"SFX source created: {sfxSource != null}");
        }
        else
        {
            Debug.Log($"SFX source already exists: {sfxSource.name}");
        }
        
        // Initialize SFX pool
        CreateSFXPool(5); // Create 5 pooled audio sources
        
        // Apply initial volume settings
        UpdateVolumeSettings();
        
        Debug.Log($"AudioManager initialized - Music: {musicSource != null}, SFX: {sfxSource != null}");
    }
    
    private void CreateSFXPool(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject sfxObj = new GameObject($"PooledSFX_{i}");
            sfxObj.transform.SetParent(transform);
            AudioSource source = sfxObj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Enqueue(source);
        }
    }
    
    #endregion
    
    #region Music Control
    
    /// <summary>
    /// Play background music with optional fade
    /// </summary>
    public void PlayMusic(AudioClip music, bool fade = true)
    {
        Debug.Log($"=== PlayMusic ENTRY === with: {(music != null ? music.name : "NULL")}");
        Debug.Log($"PlayMusic called with: {(music != null ? music.name : "NULL")}, fade: {fade}, enableMusic: {enableMusic}");
        
        if (music == null || !enableMusic) 
        {
            Debug.LogWarning($"PlayMusic failed - music: {(music != null ? "not null" : "NULL")}, enableMusic: {enableMusic}");
            return;
        }
        
        if (musicSource == null)
        {
            Debug.LogError("PlayMusic failed - musicSource is NULL!");
            return;
        }
        
        if (currentMusic == music && musicSource.isPlaying) 
        {
            Debug.Log("PlayMusic skipped - same music already playing");
            return;
        }
        
        Debug.Log($"Setting currentMusic to: {music.name}");
        currentMusic = music;
        
        if (fade && musicSource.isPlaying)
        {
            Debug.Log("Starting music fade...");
            StartMusicFade(music);
        }
        else
        {
            Debug.Log($"Playing music directly: {music.name}");
            musicSource.clip = music;
            musicSource.Play();
            Debug.Log($"Music started playing: {musicSource.isPlaying}");
        }
    }
    
    /// <summary>
    /// Stop music with optional fade
    /// </summary>
    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            StartMusicFade(null);
        }
        else
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }
    
    /// <summary>
    /// Fade between music tracks
    /// </summary>
    private void StartMusicFade(AudioClip newMusic)
    {
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeMusicCoroutine(newMusic));
    }
    
    private IEnumerator FadeMusicCoroutine(AudioClip newMusic)
    {
        Debug.Log($"FadeMusicCoroutine started with: {(newMusic != null ? newMusic.name : "NULL")}");
        float startVolume = musicSource.volume;
        Debug.Log($"Starting fade with volume: {startVolume}");
        
        // Fade out
        Debug.Log("Starting fade out...");
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * musicFadeSpeed * Time.deltaTime;
            yield return null;
        }
        
        Debug.Log("Fade out complete, stopping music");
        musicSource.Stop();
        
        // Switch music
        if (newMusic != null)
        {
            Debug.Log($"Setting new music clip: {newMusic.name}");
            musicSource.clip = newMusic;
            Debug.Log("Starting new music playback");
            musicSource.Play();
            Debug.Log($"New music started playing: {musicSource.isPlaying}");
            
            // Fade in
            Debug.Log("Starting fade in...");
            while (musicSource.volume < startVolume)
            {
                musicSource.volume += startVolume * musicFadeSpeed * Time.deltaTime;
                yield return null;
            }
            Debug.Log("Fade in complete");
        }
        else
        {
            Debug.Log("No new music to play");
        }
        
        musicSource.volume = startVolume;
        musicFadeCoroutine = null;
        Debug.Log("FadeMusicCoroutine finished");
    }
    
    #endregion
    
    #region Sound Effects
    
    /// <summary>
    /// Play a one-shot sound effect
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || !enableSFX) return;
        
        AudioSource source = GetPooledSFXSource();
        if (source != null)
        {
            source.clip = clip;
            source.volume = sfxVolume * masterVolume * volumeScale;
            source.Play();
            
            StartCoroutine(ReturnToPoolWhenFinished(source));
        }
    }
    
    /// <summary>
    /// Play sound effect at a specific world position
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float volumeScale = 1f)
    {
        if (clip == null || !enableSFX) return;
        
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume * masterVolume * volumeScale);
    }
    
    /// <summary>
    /// Play UI sound effect
    /// </summary>
    public void PlayUISFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || !enableUI) return;
        
        sfxSource.clip = clip;
        sfxSource.volume = uiVolume * masterVolume * volumeScale;
        sfxSource.Play();
    }
    
    private AudioSource GetPooledSFXSource()
    {
        if (sfxPool.Count > 0)
        {
            AudioSource source = sfxPool.Dequeue();
            activeSfxSources.Add(source);
            return source;
        }
        
        // Create new source if pool is empty
        GameObject sfxObj = new GameObject("DynamicSFX");
        sfxObj.transform.SetParent(transform);
        AudioSource newSource = sfxObj.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        activeSfxSources.Add(newSource);
        return newSource;
    }
    
    private IEnumerator ReturnToPoolWhenFinished(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        
        activeSfxSources.Remove(source);
        sfxPool.Enqueue(source);
    }
    
    #endregion
    
    #region Game-Specific Audio Events
    
    /// <summary>
    /// Handle game state changes for automatic music
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                PlayMusic(menuMusic);
                break;
            case GameState.Playing:
                // Level-specific music should already be playing from OnLevelLoaded
                // Only play the level start sound
                PlayUISFX(levelStartSound);
                break;
            case GameState.LevelComplete:
                PlayUISFX(levelCompleteSound);
                PlayMusic(victoryMusic);
                break;
            case GameState.GameOver:
            case GameState.CompleteGameOver:
                PlayUISFX(gameOverSound);
                PlayMusic(gameOverMusic);
                break;
        }
    }
    
    /// <summary>
    /// Handle level loading for level-specific music
    /// </summary>
    private void OnLevelLoaded(LevelData levelData)
    {
        Debug.Log($"OnLevelLoaded called with levelData: {(levelData != null ? levelData.levelName : "NULL")}");
        Debug.Log($"Current GameState: {GameManager.Instance?.CurrentState}");
        
        // Don't change music if we're in menu state
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Menu)
        {
            Debug.Log("In menu state - keeping menu music");
            return;
        }
        
        // PlayMusic will handle stopping current music and transitioning to new music
        // No need to call StopMusic() here as it causes conflicts with the fade system
        
        if (levelData != null && !string.IsNullOrEmpty(levelData.backgroundMusic))
        {
            Debug.Log($"Level backgroundMusic: '{levelData.backgroundMusic}'");
            AudioClip levelMusic = GetLevelMusic(levelData.backgroundMusic);
            Debug.Log($"GetLevelMusic returned: {(levelMusic != null ? levelMusic.name : "NULL")}");
            
            if (levelMusic != null)
            {
                Debug.Log($"About to call PlayMusic with: {levelMusic.name}");
                Debug.Log($"PlayMusic called with: {levelData.backgroundMusic}");
                PlayMusic(levelMusic);
            }
            else
            {
                Debug.LogWarning($"No music found for: {levelData.backgroundMusic}, using tutorial theme as fallback");
                Debug.Log($"tutorialTheme is: {(tutorialTheme != null ? tutorialTheme.name : "NULL")}");
                PlayMusic(tutorialTheme);
            }
        }
        else
        {
            Debug.LogWarning("No backgroundMusic specified in level data, using tutorial theme as fallback");
            Debug.Log($"tutorialTheme is: {(tutorialTheme != null ? tutorialTheme.name : "NULL")}");
            PlayMusic(tutorialTheme);
        }
    }
    
    /// <summary>
    /// Get the appropriate music clip based on the backgroundMusic field
    /// </summary>
    private AudioClip GetLevelMusic(string musicName)
    {
        Debug.Log($"GetLevelMusic called with: '{musicName}'");
        
        switch (musicName.ToLower())
        {
            case "tutorial_theme":
                Debug.Log($"Returning tutorialTheme: {(tutorialTheme != null ? tutorialTheme.name : "NULL")}");
                return tutorialTheme;
            case "action_theme":
                Debug.Log($"Returning actionTheme: {(actionTheme != null ? actionTheme.name : "NULL")}");
                return actionTheme;
            case "challenge_theme":
                Debug.Log($"Returning challengeTheme: {(challengeTheme != null ? challengeTheme.name : "NULL")}");
                return challengeTheme;
            case "boss_theme":
                Debug.Log($"Returning bossTheme: {(bossTheme != null ? bossTheme.name : "NULL")}");
                return bossTheme;
            default:
                Debug.LogWarning($"Unknown music theme: {musicName}");
                return null;
        }
    }
    
    /// <summary>
    /// Duck click sounds
    /// </summary>
    public void PlayDuckClickDecoy(Vector3 position)
    {
        PlaySFXAtPosition(duckClickDecoySound, position);
    }
    
    /// <summary>
    /// Duck click good sound
    /// </summary>
    public void PlayDuckClickGood(Vector3 position)
    {
        PlaySFXAtPosition(duckClickGoodSound, position);
    }
    
    #endregion
    
    #region Volume Control
    
    /// <summary>
    /// Update all volume settings
    /// </summary>
    public void UpdateVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
        
        // Update active SFX sources
        foreach (AudioSource source in activeSfxSources)
        {
            if (source != null)
                source.volume = sfxVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Set master volume
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    /// <summary>
    /// Set music volume
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    /// <summary>
    /// Set SFX volume
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    /// <summary>
    /// Toggle audio categories
    /// </summary>
    public void ToggleMusic()
    {
        enableMusic = !enableMusic;
        if (!enableMusic && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
        else if (enableMusic && !musicSource.isPlaying && currentMusic != null)
        {
            musicSource.UnPause();
        }
    }
    
    public void ToggleSFX()
    {
        enableSFX = !enableSFX;
    }
    
    public void ToggleUI()
    {
        enableUI = !enableUI;
    }
    
    #endregion
    
    #region Public Getters
    
    public bool IsMusicEnabled => enableMusic;
    public bool IsSFXEnabled => enableSFX;
    public bool IsUIEnabled => enableUI;
    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
    public AudioClip CurrentMusic => currentMusic;
    
    #endregion

    /// <summary>
    /// Debug method to test level-specific music
    /// </summary>
    [ContextMenu("Test Level Music")]
    public void TestLevelMusic()
    {
        Debug.Log("=== Testing Level-Specific Music ===");
        
        string[] testThemes = { "tutorial_theme", "action_theme", "challenge_theme", "boss_theme", "unknown_theme" };
        
        foreach (string theme in testThemes)
        {
            AudioClip music = GetLevelMusic(theme);
            Debug.Log($"Theme '{theme}': {(music != null ? music.name : "NOT FOUND")}");
        }
    }
    
    /// <summary>
    /// Comprehensive test for audio integration
    /// </summary>
    [ContextMenu("Test Audio Integration")]
    public void TestAudioIntegration()
    {
        Debug.Log("=== Testing Audio Integration ===");
        
        // Test 1: Level-specific music mapping
        Debug.Log("Test 1: Level-specific music mapping");
        TestLevelMusicMapping();
        
        // Test 2: Audio persistence simulation
        Debug.Log("Test 2: Audio persistence simulation");
        TestAudioPersistence();
        
        // Test 3: Volume settings
        Debug.Log("Test 3: Volume settings");
        TestVolumeSettings();
        
        Debug.Log("=== Audio Integration Test Complete ===");
    }
    
    private void TestLevelMusicMapping()
    {
        // Test all expected music themes
        string[] expectedThemes = { "tutorial_theme", "action_theme", "challenge_theme", "boss_theme" };
        
        foreach (string theme in expectedThemes)
        {
            AudioClip music = GetLevelMusic(theme);
            if (music != null)
            {
                Debug.Log($"✅ {theme} -> {music.name}");
            }
            else
            {
                Debug.LogWarning($"❌ {theme} -> NOT ASSIGNED");
            }
        }
        
        // Test unknown theme
        AudioClip unknownMusic = GetLevelMusic("unknown_theme");
        if (unknownMusic == null)
        {
            Debug.Log("✅ Unknown theme correctly returns null");
        }
        else
        {
            Debug.LogWarning("❌ Unknown theme should return null");
        }
    }
    
    private void TestAudioPersistence()
    {
        // Simulate checkpoint restart scenario
        Debug.Log("Simulating checkpoint restart...");
        
        // Save current state
        bool wasMusicEnabled = enableMusic;
        float savedMasterVolume = masterVolume;
        
        // Test music persistence
        Debug.Log($"Music enabled: {enableMusic}");
        Debug.Log($"Master volume: {masterVolume}");
        Debug.Log($"Current music: {(currentMusic != null ? currentMusic.name : "None")}");
        
        // Simulate restart (music should continue playing)
        if (currentMusic != null)
        {
            Debug.Log("✅ Music should persist across checkpoint restarts");
        }
        else
        {
            Debug.Log("ℹ️ No music currently playing");
        }
        
        // Restore state
        enableMusic = wasMusicEnabled;
        masterVolume = savedMasterVolume;
    }
    
    private void TestVolumeSettings()
    {
        Debug.Log($"Master Volume: {masterVolume}");
        Debug.Log($"Music Volume: {musicVolume}");
        Debug.Log($"SFX Volume: {sfxVolume}");
        Debug.Log($"UI Volume: {uiVolume}");
        
        // Test volume calculations
        float calculatedMusicVolume = musicVolume * masterVolume;
        Debug.Log($"Calculated Music Volume: {calculatedMusicVolume}");
        
        if (calculatedMusicVolume > 0)
        {
            Debug.Log("✅ Volume calculations working");
        }
        else
        {
            Debug.LogWarning("⚠️ Volume may be too low");
        }
    }
}