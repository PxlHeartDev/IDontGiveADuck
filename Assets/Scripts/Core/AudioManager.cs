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
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    
    // Current music tracking
    private AudioClip currentMusic;
    
    #region Unity Lifecycle
    
    void Awake()
    {
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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            GameManager.Instance.OnLevelLoaded += OnLevelLoaded;
        }
        
        PlayMusic(menuMusic);
    }
    
    void OnDestroy()
    {
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
        
        UpdateVolumeSettings();
    }
    
    #endregion
    
    #region Music Control
    
    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;
        
        if (currentMusic == music && musicSource.isPlaying) return;
        
        currentMusic = music;
        musicSource.clip = music;
        musicSource.Play();
    }
    
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
    
    public void PlaySFXAtPosition(AudioClip clip, Vector3 position)
    {
        if (clip == null || sfxSource == null) return;
        
        float duckVolumeMultiplier = 20.0f;
        float finalVolume = sfxVolume * masterVolume * duckVolumeMultiplier;
        
        sfxSource.clip = clip;
        sfxSource.volume = finalVolume;
        sfxSource.Play();
    }
    
    public void PlayUISFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        
        sfxSource.clip = clip;
        sfxSource.volume = sfxVolume * masterVolume;
        sfxSource.Play();
    }
    
    #endregion
    
    #region Game-Specific Audio Events
    
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
    
    private void OnLevelLoaded(LevelData levelData)
    {
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
                PlayMusic(tutorialTheme);
            }
        }
        else
        {
            PlayMusic(tutorialTheme);
        }
    }
    
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
    
    public void PlayDuckClickDecoy(Vector3 position)
    {
        if (duckClickDecoySound != null)
        {
            PlaySFXAtPosition(duckClickDecoySound, position);
        }
    }
    
    public void PlayDuckClickGood(Vector3 position)
    {
        if (duckClickGoodSound != null)
        {
            PlaySFXAtPosition(duckClickGoodSound, position);
        }
    }
    
    #endregion
    
    #region Volume Control
    
    public void UpdateVolumeSettings()
    {
        if (musicSource != null)
            musicSource.volume = musicVolume * masterVolume;
        
        if (sfxSource != null)
            sfxSource.volume = sfxVolume * masterVolume;
    }
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateVolumeSettings();
    }
    
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