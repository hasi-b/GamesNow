using UnityEngine;
using System.Collections;

public class BackgroundAudio : MonoBehaviour
{
    public static BackgroundAudio instance;
    public AudioClip backgroundMusic;
    public AudioClip backgroundMusicAnger;// The default background music
    public AudioClip level2Audio; // The new background music for level 2
    public float volume = 0.5f; // Volume of the background music (0.0 to 1.0)
    public bool loop = true; // Should the music loop?

    private AudioSource audioSource;

    void Awake()
    {
        instance = this;
        // Ensure there's only one instance of this script (singleton pattern)
        if (FindObjectsByType<BackgroundAudio>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Persist this object across scenes
        DontDestroyOnLoad(gameObject);

        // Add and configure the AudioSource component
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = loop;

        // Play the background music
        audioSource.Play();
    }

    // Method to fade between two audio clips
    public void FadeBetweenTracks(AudioClip newTrack, float fadeDuration)
    {
        StartCoroutine(FadeMusic(newTrack, fadeDuration));
    }

    private IEnumerator FadeMusic(AudioClip newTrack, float fadeDuration)
    {
        // Fade out the current music
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }

        // Change the music and start playing it
        audioSource.clip = newTrack;
        audioSource.Play();

        // Fade in the new track
        while (audioSource.volume < volume)
        {
            audioSource.volume += startVolume * Time.deltaTime / fadeDuration;
            yield return null;
        }
    }

    public void StartAngerMusic() {

        audioSource.clip = backgroundMusicAnger;
        audioSource.Play();

    }
}
