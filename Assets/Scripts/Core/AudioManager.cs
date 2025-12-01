using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AudioManager - Centralised audio system for the game
/// 
/// This class demonstrates several important game development concepts:
/// - Singleton Pattern: Ensures only one audio manager exists
/// - Event-Driven Architecture: Responds to game state changes
/// - Audio Source Management: Separate sources for music and SFX
/// - Volume Control System: Layered volume controls (master, music, SFX)
/// - Dynamic Music Switching: Changes music based on game state and levels
/// </summary>
public class AudioManager : MonoBehaviour
{
    // Singleton pattern - accessible from anywhere in the game
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;       // Dedicated source for background music
    [SerializeField] private List<AudioSource> sfxSource;   // Dedicated sources for sound effects
    [SerializeField] private AudioSource sfxSourceUI;       // Dedicated source for UI sound effects

    [Header("Background Music")]
    [SerializeField] private AudioClip menuMusic;      // Music for main menu
    [SerializeField] private AudioClip gameOverMusic;  // Music for game over screen
    [SerializeField] private AudioClip victoryMusic;   // Music for level completion
    
    [Header("Level-Specific Music")]
    [SerializeField] private AudioClip tutorialTheme;  // Music for tutorial levels
    [SerializeField] private AudioClip actionTheme;    // Music for action levels
    [SerializeField] private AudioClip challengeTheme; // Music for challenging levels
    [SerializeField] private AudioClip bossTheme;      // Music for boss levels
    
    [Header("UI Sounds")]
    [SerializeField] private AudioClip levelStartSound;    // Sound when level begins
    [SerializeField] private AudioClip levelCompleteSound; // Sound when level is won
    [SerializeField] private AudioClip gameOverSound;      // Sound when level is lost
    
    [Header("Duck Sounds")]
    [SerializeField] private AudioClip duckClickDecoySound; // Sound when clicking decoy 
    [SerializeField] private AudioClip duckClickDecoySound2; // Sound when clicking decoy duck
    [SerializeField] private AudioClip duckClickGoodSound;  // Sound when clicking good duck
    [SerializeField] private AudioClip duckFavouriteSound;  // Sound when feeding a duck their favourite berry
    [SerializeField] private AudioClip duckNotFavourite;    // Sound when breaking the combo
    
    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;   // Overall volume control
    [Range(0f, 1f)] public float musicVolume = 0.5f;  // Music-specific volume
    [Range(0f, 1f)] public float sfxVolume = 1f;      // Sound effects volume

    private int curSFXSource = 0;
    private int favouriteCombo = 0;
    
    // Track current music to avoid restarting the same track
    private AudioClip currentMusic;
    
    #region Unity Lifecycle
    
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep audio manager alive across scenes
            InitializeAudioManager();
        }
        else
        {
            // Destroy duplicate instances
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Subscribe to game events for automatic audio responses
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
        // Unsubscribe from events to prevent memory leaks
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            GameManager.Instance.OnLevelLoaded -= OnLevelLoaded;
        }
    }
    
    #endregion
    
    #region Initialisation
    
    /// <summary>
    /// Sets up the audio system with proper AudioSource components
    /// 
    /// Creates separate AudioSource objects for music and SFX if they don't exist
    /// This separation allows independent control of music and sound effects
    /// </summary>
    private void InitializeAudioManager()
    {
        // Create music source if it doesn't exist
        if (musicSource == null)
        {
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;           // Music should loop continuously
            musicSource.playOnAwake = false;   // Don't start playing immediately
        }

        sfxSource = new();
        for (int i = 0; i < 10; i++)
        {
            GameObject sfxObj = new GameObject("SFXSource" + i);
            sfxObj.transform.SetParent(transform);
            sfxSource.Add(sfxObj.AddComponent<AudioSource>());
            sfxSource[i].playOnAwake = false;     // Don't start playing immediately
        }
        

        // Create UI SFX source if it doesn't exist
        if (sfxSourceUI == null)
        {
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            sfxSourceUI = sfxObj.AddComponent<AudioSource>();
            sfxSourceUI.playOnAwake = false;   // Don't start playing immediately
        }


        SetMusicVolume(0.07f);
        // Apply initial volume settings
        UpdateVolumeSettings();
    }
    
    #endregion
    
    #region Music Control
    
    /// <summary>
    /// Plays background music with smart switching
    /// 
    /// Features:
    /// - Prevents restarting the same music track
    /// - Handles null audio clips gracefully
    /// - Automatically manages the music source
    /// </summary>
    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;
        
        // Don't restart the same music if it's already playing
        if (currentMusic == music && musicSource.isPlaying) return;
        
        currentMusic = music;
        musicSource.clip = music;
        musicSource.Play();
    }
    
    /// <summary>
    /// Stops the current background music
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
    /// Plays sound effects at a specific position in 3D space
    /// 
    /// Used for duck sounds that should appear to come from the duck's location
    /// Includes a volume multiplier for duck sounds to make them more audible
    /// </summary>
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position, float pitchVariation = 0.0f, float pitch = 1.0f)
    {
        if (clip == null || sfxSource == null) return;

        // Duck sounds need to be quieter than regular SFX
        float duckVolumeMultiplier = 0.2f;
        float finalVolume = sfxVolume * masterVolume * duckVolumeMultiplier;

        sfxSource[curSFXSource].pitch = pitch + Random.Range(-pitchVariation, pitchVariation);
        sfxSource[curSFXSource].clip = clip;
        sfxSource[curSFXSource].volume = finalVolume;
        sfxSource[curSFXSource].Play();

        curSFXSource++;
        if (curSFXSource >= 10) 
            curSFXSource = 0;
    }

    /// <summary>
    /// Plays UI sound effects (not positional)
    /// 
    /// Used for menu sounds, level start/complete sounds
    /// These sounds don't need 3D positioning
    /// </summary>
    public void PlayUISFX(AudioClip clip, float pitchVariation = 0.0f)
    {
        if (clip == null || sfxSource == null) return;

        sfxSourceUI.pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);
        sfxSourceUI.clip = clip;
        sfxSourceUI.volume = sfxVolume * masterVolume;
        sfxSourceUI.Play();
    }
    
    #endregion
    
    #region Game-Specific Audio Events
    
    /// <summary>
    /// Responds to game state changes with appropriate audio
    /// 
    /// This is an example of event-driven programming:
    /// The audio system automatically responds to game events
    /// without needing direct calls from other systems
    /// </summary>
    private void OnGameStateChanged(GameState newState)
    {
        switch (newState)
        {
            case GameState.Menu:
                PlayMusic(menuMusic);
                break;
            case GameState.Playing:
                favouriteCombo = 0;
                //PlayUISFX(levelStartSound);
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
    /// Changes music based on the loaded level
    /// 
    /// Reads the backgroundMusic field from level data
    /// and switches to appropriate music for that level type
    /// </summary>
    private void OnLevelLoaded(LevelData levelData)
    {
        // Don't change music if we're still in the menu
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
                // Fallback to tutorial theme if music not found
                PlayMusic(tutorialTheme);
            }
        }
        else
        {
            // Default to tutorial theme if no music specified
            PlayMusic(tutorialTheme);
        }
    }
    
    /// <summary>
    /// Converts music name strings to actual AudioClip objects
    /// 
    /// This allows level designers to specify music by name
    /// rather than having to reference AudioClip objects directly
    /// </summary>
    private AudioClip GetLevelMusic(string musicName)
    {
        if (string.IsNullOrEmpty(musicName))
            return null;
            
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
    /// Plays sound when player clicks a decoy duck
    /// </summary>
    public void PlayDuckClickDecoy(Vector3 position)
    {
        if (duckClickDecoySound != null)
        {
            PlaySFXAtPosition(duckClickDecoySound, position, 0.2f);
            PlaySFXAtPosition(duckClickDecoySound2, position, 0.2f);
        }
    }
    
    /// <summary>
    /// Plays sound when player clicks a good duck
    /// </summary>
    public void PlayDuckClickGood(Vector3 position, bool usedFavourite)
    {
        if (duckClickGoodSound != null)
        {
            PlaySFXAtPosition(duckClickGoodSound, position, 0.2f);
        }

        if (usedFavourite)
        {
            PlaySFXAtPosition(duckFavouriteSound, position, 0.0f, 1.0f + Mathf.Clamp01(favouriteCombo * 0.1f));
            favouriteCombo++;
        }
        else if (favouriteCombo > 0)
        {
            PlaySFXAtPosition(duckNotFavourite, position);
            favouriteCombo = 0;
        }
    }
    
    #endregion
    
    #region Volume Control
    
    /// <summary>
    /// Updates all audio sources with current volume settings
    /// 
    /// Volume is calculated as: sourceVolume * masterVolume
    /// This creates a layered volume control system
    /// </summary>
    public void UpdateVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;

        foreach (AudioSource source in sfxSource)
        {
            if (source != null)
                source.volume = sfxVolume * masterVolume;
        }
    }
    
    /// <summary>
    /// Sets the master volume (affects all audio)
    /// 
    /// Mathf.Clamp01 ensures the value stays between 0 and 1
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    /// <summary>
    /// Sets the music volume (affects only background music)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    /// <summary>
    /// Sets the SFX volume (affects only sound effects)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    #endregion
    
    #region Public Getters
    
    // Properties to check audio state from other systems
    public bool IsMusicPlaying => musicSource != null && musicSource.isPlaying;
    public AudioClip CurrentMusic => currentMusic;
    
    #endregion
}