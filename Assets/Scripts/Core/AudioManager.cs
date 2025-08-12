using UnityEngine;

/// <summary>
/// Simple audio management system for the duck game
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
    [Range(0f, 1f)] public float musicVolume = 0.2f;  // Much lower music volume
    [Range(0f, 1f)] public float sfxVolume = 1f;      // Higher SFX volume
    
    // Current music tracking
    private AudioClip currentMusic;
    
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
        // Subscribe to game events
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
        // Create audio sources if not assigned
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }
        
        if (sfxSource == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
        
        // Apply initial volume settings
        UpdateVolumeSettings();
    }
    
    #endregion
    
    #region Music Control
    
    /// <summary>
    /// Play background music
    /// </summary>
    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;
        
        if (currentMusic == music && musicSource.isPlaying) return;
        
        currentMusic = music;
        musicSource.clip = music;
        musicSource.Play();
    }
    
    /// <summary>
    /// Stop music
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
            currentMusic = null;
        }
    }
    
    #endregion
    
    #region Sound Effects
    
    /// <summary>
    /// Play sound effect at a specific world position
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        // Increase volume for duck sounds to make them more prominent
        float duckVolumeMultiplier = 10.0f; // 10x louder than regular SFX
        float finalVolume = sfxVolume * masterVolume * duckVolumeMultiplier;
        Debug.Log($"PlaySFXAtPosition: clip={clip.name}, volume={finalVolume}, sfxVolume={sfxVolume}, masterVolume={masterVolume}");
        AudioSource.PlayClipAtPoint(clip, position, finalVolume);
    }
    
    /// <summary>
    /// Play UI sound effect
    /// </summary>
    public void PlayUISFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        
        sfxSource.clip = clip;
        sfxSource.volume = sfxVolume * masterVolume;
        sfxSource.Play();
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
                PlayUISFX(levelStartSound);
                break;
            case GameState.LevelComplete:
                PlayUISFX(levelCompleteSound);
                PlayMusic(victoryMusic);
                break;
            case GameState.GameOver:
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
        // Don't change music if we're in menu state
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Menu)
        {
            return;
        }
        
        if (levelData != null && !string.IsNullOrEmpty(levelData.backgroundMusic))
        {
            AudioClip levelMusic = GetLevelMusic(levelData.backgroundMusic);
            if (levelMusic != null)
            {
                PlayMusic(levelMusic);
            }
            else
            {
                PlayMusic(tutorialTheme); // Fallback
            }
        }
        else
        {
            PlayMusic(tutorialTheme); // Fallback
        }
    }
    
    /// <summary>
    /// Get the appropriate music clip based on the backgroundMusic field
    /// </summary>
    private AudioClip GetLevelMusic(string musicName)
    {
        switch (musicName.ToLower())
        {
            case "tutorial_theme":
                return tutorialTheme;
            case "action_theme":
                return actionTheme;
            case "challenge_theme":
                return challengeTheme;
            case "boss_theme":
                return bossTheme;
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Duck click sounds
    /// </summary>
    public void PlayDuckClickDecoy(Vector3 position)
    {
        Debug.Log("PlayDuckClickDecoy called");
        if (duckClickDecoySound == null)
        {
            Debug.LogError("duckClickDecoySound is NULL! Assign it in the Inspector.");
            return;
        }
        Debug.Log($"Playing decoy sound: {duckClickDecoySound.name}");
        PlaySFXAtPosition(duckClickDecoySound, position);
    }
    
    /// <summary>
    /// Duck click good sound
    /// </summary>
    public void PlayDuckClickGood(Vector3 position)
    {
        Debug.Log("PlayDuckClickGood called");
        if (duckClickGoodSound == null)
        {
            Debug.LogError("duckClickGoodSound is NULL! Assign it in the Inspector.");
            return;
        }
        Debug.Log($"Playing good duck sound: {duckClickGoodSound.name}");
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
    
    #endregion
    
    #region Public Getters
    
    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
    public AudioClip CurrentMusic => currentMusic;
    
    #endregion
}