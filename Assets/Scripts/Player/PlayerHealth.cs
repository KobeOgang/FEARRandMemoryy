using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Blood Stain Overlay")]
    public Image bloodStainOverlay;

    [Header("Damage Effects")]
    public float damageFlashDuration = 0.5f;
    public float bloodStainFadeSpeed = 2f;

    [Header("Regeneration")]
    public bool canRegenerate = true;
    public float regenDelay = 5f;
    public float regenRate = 10f;
    private float timeSinceLastDamage;

    [Header("Scene Transition")]
    public string gameOverSceneName = "GameOver";
    public float fadeToBlackDuration = 2f;

    private PlayerController playerController;
    private bool isDead = false;
    private bool isPlayingDeathAnimation = false;
    private float targetBloodAlpha = 0f;

    private void Start()
    {
        currentHealth = maxHealth;

        // Get PlayerController reference
        playerController = GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerHealth: PlayerController component not found!");
        }

        // Setup blood stain overlay
        if (bloodStainOverlay != null)
        {
            Color bloodColor = bloodStainOverlay.color;
            bloodColor.a = 0f;
            bloodStainOverlay.color = bloodColor;
            bloodStainOverlay.raycastTarget = false;
        }


        UpdateBloodStainOpacity();
    }

    private void Update()
    {
        // Handle death animation completion
        if (isPlayingDeathAnimation && playerController != null)
        {
            if (playerController.IsDeathAnimationComplete())
            {
                CompleteDeathSequence();
            }
        }

        if (isDead) return;

        // Handle health regeneration
        if (canRegenerate && currentHealth < maxHealth)
        {
            timeSinceLastDamage += Time.deltaTime;

            if (timeSinceLastDamage >= regenDelay)
            {
                RegenerateHealth();
            }
        }

        // Smoothly update blood stain opacity
        if (bloodStainOverlay != null)
        {
            Color currentColor = bloodStainOverlay.color;
            currentColor.a = Mathf.Lerp(currentColor.a, targetBloodAlpha, Time.deltaTime * bloodStainFadeSpeed);
            bloodStainOverlay.color = currentColor;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isPlayingDeathAnimation) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        timeSinceLastDamage = 0f;

        Debug.Log($"Player took {damage} damage. Health: {currentHealth}/{maxHealth}");

        UpdateBloodStainOpacity();
        ShowDamageEffect();

        if (currentHealth <= 0f)
        {
            StartDeathSequence();
        }
    }

    private void StartDeathSequence()
    {
        if (isPlayingDeathAnimation || isDead) return;

        isPlayingDeathAnimation = true;
        targetBloodAlpha = 1f; 

        if (playerController != null)
        {
            playerController.TriggerDeath();
        }
        else
        {
            CompleteDeathSequence();
        }

        Debug.Log("Death sequence started - playing animation...");
    }

    private void CompleteDeathSequence()
    {
        if (isDead) return;

        isDead = true;
        isPlayingDeathAnimation = false;

        if (playerController != null)
        {
            playerController.SetDeathComplete();
        }

        Debug.Log("Death animation complete - starting fade to GameOver scene");

        // Disable player input during transition
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Start fade to black and scene transition
        StartCoroutine(FadeToBlackAndLoadGameOver());
    }

    private IEnumerator FadeToBlackAndLoadGameOver()
    {
        // Create fade overlay
        GameObject fadeObj = new GameObject("GameOverFadeOverlay");
        Canvas canvas = fadeObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Make sure it's on top of everything

        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0); // Start transparent
        fadeImage.raycastTarget = false; // Don't block interactions during fade

        // Make it cover the entire screen
        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Fade to black
        float elapsedTime = 0f;
        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            float alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeToBlackDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure completely black
        fadeImage.color = Color.black;

        // Small delay to ensure fade is visible
        yield return new WaitForSecondsRealtime(0.1f);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load the GameOver scene
        SceneManager.LoadScene(gameOverSceneName);
    }


    public void Heal(float healAmount)
    {
        if (isDead || isPlayingDeathAnimation) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"Player healed for {healAmount}. Health: {currentHealth}/{maxHealth}");
        UpdateBloodStainOpacity();
    }

    private void RegenerateHealth()
    {
        float regenAmount = regenRate * Time.deltaTime;
        currentHealth += regenAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateBloodStainOpacity();
    }

    private void UpdateBloodStainOpacity()
    {
        if (bloodStainOverlay == null) return;

        float healthPercentageLost = 1f - (currentHealth / maxHealth);
        targetBloodAlpha = Mathf.Clamp01(healthPercentageLost * 0.8f);
    }

    private void ShowDamageEffect()
    {
        if (bloodStainOverlay != null)
        {
            StartCoroutine(DamageFlashEffect());
        }
    }

    private IEnumerator DamageFlashEffect()
    {
        float originalAlpha = targetBloodAlpha;
        targetBloodAlpha = Mathf.Clamp01(originalAlpha + 0.3f);

        yield return new WaitForSeconds(damageFlashDuration);

        targetBloodAlpha = originalAlpha;
    }

    public void Respawn()
    {
        isDead = false;
        isPlayingDeathAnimation = false;
        currentHealth = maxHealth;
        UpdateBloodStainOpacity();

        if (playerController != null)
        {
            playerController.ResetDeath();
        }

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Player respawned!");
    }

    public bool IsAlive()
    {
        return !isDead && !isPlayingDeathAnimation;
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
