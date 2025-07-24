using UnityEngine;

/// <summary>
/// Decoy duck that penalizes players when clicked
/// </summary>
public class DecoyDuck : BaseDuck
{
    [Header("Decoy Duck Settings")]
    [SerializeField] private ParticleSystem penaltyParticles;
    [SerializeField] private AudioClip penaltySound;
    [SerializeField] private int timePenalty = 3; // seconds to subtract
    [SerializeField] private GameObject penaltyTextPrefab; // Optional floating text
    
    [Header("Visual Distinction")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color decoyColor = Color.red;
    [SerializeField] private bool subtleVisualDifference = true; // Make it harder to distinguish
    
    #region Initialization Override
    
    /// <summary>
    /// Initialize decoy duck with custom properties
    /// </summary>
    public override void Initialize(float customLifetime = -1, int customPointValue = -1)
    {
        base.Initialize(customLifetime, customPointValue);
        
        // Apply visual distinction if enabled
        if (spriteRenderer != null && !subtleVisualDifference)
        {
            spriteRenderer.color = decoyColor;
        }
        else if (spriteRenderer != null && subtleVisualDifference)
        {
            // Very subtle tint - harder to spot
            spriteRenderer.color = Color.Lerp(Color.white, decoyColor, 0.1f);
        }
    }
    
    #endregion
    
    #region Abstract Implementation
    
    protected override void OnClicked()
    {
        Debug.Log($"Decoy duck clicked! Applied {timePenalty}s penalty");
        
        // Notify game manager about penalty
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDecoyDuckClicked(this);
        }
        
        // Play penalty feedback
        PlayPenaltyEffects();
        
        // Destroy duck
        DestroyDuck();
    }
    
    protected override void OnLifetimeExpired()
    {
        Debug.Log("Decoy duck expired naturally - no penalty");
        
        // Notify game manager (no penalty for natural expiration)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDecoyDuckExpired(this);
        }
        
        // No penalty effects for natural expiration
    }
    
    #endregion
    
    #region Virtual Overrides
    
    protected override void OnDuckSpawned()
    {
        base.OnDuckSpawned();
        
        // Decoy duck specific spawn behavior
        // Could add subtle spawn differences
        
        // Ensure proper tag for identification
        gameObject.tag = "DecoyDuck";
        
        // Optional: Add subtle behavioral differences
        if (subtleVisualDifference)
        {
            AddSubtleBehavioralDifferences();
        }
    }
    
    protected override void HandleMovement()
    {
        base.HandleMovement();
        
        // Decoy ducks could have slightly different movement patterns
        // This could help players learn to distinguish them
        if (moveSpeed > 0)
        {
            // Example: Decoy ducks move in a slightly different pattern
            float wiggle = Mathf.Sin(Time.time * 2f) * 0.1f;
            transform.position += Vector3.right * wiggle * Time.deltaTime;
        }
    }
    
    #endregion
    
    #region Decoy Duck Specific Methods
    
    /// <summary>
    /// Play penalty effects when clicked
    /// </summary>
    private void PlayPenaltyEffects()
    {
        // Use AudioManager for sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDuckClickDecoy(transform.position);
        }
        
        // Particle effect (different from good duck)
        if (penaltyParticles != null)
        {
            ParticleSystem effect = Instantiate(penaltyParticles, transform.position, transform.rotation);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
        
        // Floating penalty text (optional)
        if (penaltyTextPrefab != null)
        {
            GameObject penaltyText = Instantiate(penaltyTextPrefab, transform.position, Quaternion.identity);
            // Assume the prefab has a script to handle floating animation
        }
        
        // Screen shake or other dramatic feedback
        if (Camera.main != null)
        {
            StartCoroutine(ScreenShake());
        }
    }
    
    /// <summary>
    /// Add subtle differences to make decoys learnable but not obvious
    /// </summary>
    private void AddSubtleBehavioralDifferences()
    {
        // Example: Decoy ducks spawn slightly closer to edges
        // Or have slightly different timing patterns
        // This gives observant players a chance to learn the differences
        
        // Slight scale difference (barely noticeable)
        float scaleVariation = Random.Range(0.95f, 1.05f);
        transform.localScale *= scaleVariation;
        
        // Slight rotation variation
        transform.rotation *= Quaternion.Euler(0, 0, Random.Range(-2f, 2f));
    }
    
    /// <summary>
    /// Screen shake effect for penalty feedback
    /// </summary>
    private System.Collections.IEnumerator ScreenShake()
    {
        Vector3 originalPosition = Camera.main.transform.position;
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-0.1f, 0.1f);
            float y = Random.Range(-0.1f, 0.1f);
            
            Camera.main.transform.position = originalPosition + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            
            yield return null;
        }
        
        Camera.main.transform.position = originalPosition;
    }
    
    #endregion
    
    #region Size Variants (for different prefabs)
    
    /// <summary>
    /// Configure duck as large decoy
    /// </summary>
    public void SetAsLarge()
    {
        timePenalty = 2; // Less penalty for large (easier to avoid)
        transform.localScale = Vector3.one * 1.2f;
        gameObject.name = "DecoyDuck_Large";
    }
    
    /// <summary>
    /// Configure duck as medium decoy
    /// </summary>
    public void SetAsMedium()
    {
        timePenalty = 3; // Standard penalty
        transform.localScale = Vector3.one * 1.0f;
        gameObject.name = "DecoyDuck_Medium";
    }
    
    /// <summary>
    /// Configure duck as small decoy
    /// </summary>
    public void SetAsSmall()
    {
        timePenalty = 5; // Higher penalty for small (harder to distinguish)
        transform.localScale = Vector3.one * 0.7f;
        gameObject.name = "DecoyDuck_Small";
    }
    
    #endregion
}