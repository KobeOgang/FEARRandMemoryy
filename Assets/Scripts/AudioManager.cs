using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXsource;
    [SerializeField] private AudioSource FootstepsSource;

    [Header("BGM Clips")]
    public AudioClip mainMenuMusic;
    public AudioClip gameBGM;
    public AudioClip gameOverScreenMusic; 

    [Header("SFX Clips")]
    public AudioClip doorOpen; 
    public AudioClip asteroidExplosionSFX; 
    public AudioClip earthImpactSFX;
    public AudioClip gameOverSFX;
    public AudioClip forceFieldHit;
    public AudioClip forceFieldOn;
    public AudioClip earthHealSFX;
    public AudioClip wrongAnswerSFX;

    [Header("Fade Settings")]
    [Tooltip("Duration for music fade transitions (in seconds)")]
    [Range(0.5f, 5f)]
    public float fadeDuration = 1.5f;

    private AudioClip previousBGM;
    public bool isPlayingDialogueMusic = false;
    private Coroutine currentFadeCoroutine;


    public static AudioManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        PlayMainMenuMusic();
    }

    private void Start()
    {
        PlayMainMenuMusic();
        Debug.Log("Save file path: " + Application.persistentDataPath);
    }

    public void PlayMainMenuMusic()
    {
        musicSource.clip = mainMenuMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameBGM()
    {
        musicSource.clip = gameBGM;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlayGameOverScreenMusic()
    {
        musicSource.clip = gameOverScreenMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        SFXsource.Stop();
    }

    // methods for playing SFX
    public void PlayDoorOpenSFX()
    {
        PlaySFX(doorOpen);
    }

    public void PlayForceFieldHitSFX()
    {
        PlaySFX(forceFieldHit);
    }
    public void PlayForceFieldOnSFX()
    {
        PlaySFX(forceFieldOn);
    }
    public void PlayEarthHealSFX()
    {
        PlaySFX(earthHealSFX);
    }

    public void PlayAsteroidExplosionSFX()
    {
        PlaySFX(asteroidExplosionSFX);
    }

    public void PlayEarthImpactSFX()
    {
        PlaySFX(earthImpactSFX);
    }

    public void PlayGameOverSFX()
    {
        PlaySFX(gameOverSFX);
    }

    public void PlayWrongAnswerSFX()
    {
        PlaySFX(wrongAnswerSFX);
    }


    private void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            SFXsource.PlayOneShot(clip);
        }
    }

    public void PlayClip(AudioClip clip)
    {
        if (clip != null)
        {
            SFXsource.PlayOneShot(clip);
        }
    }

    public void PlayClipWithRandomPitch(AudioClip clip, float minPitch, float maxPitch)
    {
        if (clip != null)
        {
            FootstepsSource.pitch = Random.Range(minPitch, maxPitch);
            FootstepsSource.PlayOneShot(clip);
            //FootstepsSource.pitch = 1f;
        }
    }

    public void StartDialogueBGM(AudioClip customBGM)
    {
        if (isPlayingDialogueMusic)
        {
            Debug.LogWarning("Dialogue BGM is already playing! Ignoring new request.");
            return;
        }

        previousBGM = musicSource.clip;
        isPlayingDialogueMusic = true;

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        if (customBGM == null)
        {
            return; 
        }

        currentFadeCoroutine = StartCoroutine(FadeToDialogueMusic(customBGM));
    }

    public void EndDialogueBGM()
    {
        if (!isPlayingDialogueMusic)
        {
            return; 
        }

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Restore previous music
        currentFadeCoroutine = StartCoroutine(FadeBackToPreviousMusic());
        isPlayingDialogueMusic = false;
    }

    private IEnumerator FadeToDialogueMusic(AudioClip dialogueBGM)
    {
        float startVolume = musicSource.volume;

        // Fade out current music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Switch to dialogue music
        musicSource.clip = dialogueBGM;
        musicSource.loop = true;
        musicSource.Play();

        // Fade in dialogue music
        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.volume = startVolume;
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Just fades out current music (for dialogues with no custom BGM)
    /// </summary>
    private IEnumerator FadeOutCurrentMusic()
    {
        float startVolume = musicSource.volume;

        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.Stop();
        currentFadeCoroutine = null;
    }

    /// <summary>
    /// Fades back to the music that was playing before dialogue
    /// </summary>
    private IEnumerator FadeBackToPreviousMusic()
    {
        float targetVolume = 0.7f; // Assume full volume for restored music
        float startVolume = musicSource.volume;

        // Fade out dialogue music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Restore previous music
        if (previousBGM != null)
        {
            musicSource.clip = previousBGM;
            musicSource.loop = true;
            musicSource.Play();

            // Fade in previous music
            musicSource.volume = 0;
            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * Time.deltaTime / fadeDuration;
                yield return null;
            }
            musicSource.volume = targetVolume;
        }

        previousBGM = null;
        currentFadeCoroutine = null;
    }

    public void ChangeBGMWithFade(AudioClip newBGM)
    {
        if (newBGM == null)
        {
            Debug.LogWarning("Attempted to change BGM to null clip!");
            return;
        }

        if (musicSource.clip == newBGM)
        {
            Debug.Log("BGM is already playing the requested clip.");
            return;
        }

        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        currentFadeCoroutine = StartCoroutine(FadeToBGM(newBGM));
    }

    private IEnumerator FadeToBGM(AudioClip newBGM)
    {
        float startVolume = musicSource.volume;

        // Fade out current music
        while (musicSource.volume > 0)
        {
            musicSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Switch to new BGM
        musicSource.clip = newBGM;
        musicSource.loop = true;
        musicSource.Play();

        // Fade in new music
        while (musicSource.volume < startVolume)
        {
            musicSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        musicSource.volume = startVolume;
        currentFadeCoroutine = null;
    }
}
