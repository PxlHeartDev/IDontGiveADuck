using UnityEngine;

/// <summary>
/// Good duck that players should click for points
/// Save this as: Assets/Scripts/Gameplay/Ducks/GoodDuck.cs
/// </summary>
public class GoodDuck : BaseDuck
{
    [Header("Good Duck Settings")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private GameObject successTextPrefab; // Optional floating text
    
    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color lowLifetimeColor = Color.yellow;
    private Color originalColor;
    
    protected override void Start()
    {
        base.Start();
        
        // Store original color for lifetime warnings
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    #region Abstract Implementation
    
    protected override void OnClicked()
    {
        Debug.Log($"Good duck clicked! Awarded {pointValue} points");
        
        // Notify game manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckClicked(this);
        }
        
        // Play success feedback
        PlaySuccessEffects();
        
        // Destroy duck
        DestroyDuck();
    }
    
    protected override void OnLifetimeExpired()
    {
        Debug.Log("Good duck expired - player missed it!");
        
        // Notify game manager about missed duck
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGoodDuckMissed(this);
        }
        
        // No special effects for missed ducks
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Good duck specific spawn behavior
        // Ensure proper tag for identification
        gameObject.tag = "GoodDuck";
        
        // Play spawn sound through AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckSpawn(transform.position);
        }
    }
    
    protected override void OnLifetimeLow()
    {
        base.OnLifetimeLow();
        
        // Visual warning that duck is about to expire
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(originalColor, lowLifetimeColor, 
                1f - (currentLifetime / 1f)); // Fade over last second
        }
        
        // Could add blinking effect, urgency sound, etc.
        StartBlinking();
    }
    
    #endregion
    
    #region Initialization Override
    
    /// <summary>
    /// Initialize good duck with custom properties
    /// </summary>
    public override void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        base.Initialize(customLifetime, customPointValue);
        
        // Good duck specific initialization
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }
    
    #endregion
    
    #region Good Duck Specific Methods
    
    /// <summary>
    /// Play success effects when clicked
    /// </summary>
    private void PlaySuccessEffects()
    {
        // Use AudioManager for sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickGood(transform.position);
        }
        
        // Particle effect
        if (successParticles != null)
        {
            ParticleSystem effect = Instantiate(successParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Floating score text (optional)
        if (successTextPrefab != null)
        {
            GameObject scoreText = Instantiate(successTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
    }
    
    /// <summary>
    /// Start blinking effect when lifetime is low
    /// </summary>
    private void StartBlinking()
    {
        if (spriteRenderer != null)
        {
            StartCoroutine(BlinkEffect());
        }
    }
    
    /// <summary>
    /// Coroutine for blinking effect
    /// </summary>
    private System.Collections.IEnumerator BlinkEffect()
    {
        while (currentLifetime > 0 && !isClicked)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = lowLifetimeColor;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = originalColor;
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                break;
            }
        }
    }
    
    #endregion
    
    #region Size Variants (for different prefabs)
    
    /// <summary>
    /// Configure duck as large size
    /// </summary>
    public void SetAsLarge()
    {
        pointValue = 1;
        transform.localScale = Vector3.one * 2.0f; // Increased from 1.2f
        gameObject.name = "GoodDuck_Large";
        
        // Update collider size
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = Vector2.one * 2.0f;
        }
    }
    
    /// <summary>
    /// Configure duck as medium size
    /// </summary>
    public void SetAsMedium()
    {
        pointValue = 2;
        transform.localScale = Vector3.one * 1.4f; // Increased from 1.0f
        gameObject.name = "GoodDuck_Medium";
        
        // Update collider size
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = Vector2.one * 1.4f;
        }
    }
    
    /// <summary>
    /// Configure duck as small size
    /// </summary>
    public void SetAsSmall()
    {
        pointValue = 5;
        transform.localScale = Vector3.one * 1.0f; // Increased from 0.7f
        gameObject.name = "GoodDuck_Small";
        
        // Update collider size
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = Vector2.one * 1.0f;
        }
    }
    
    #endregion
}