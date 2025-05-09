using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public AudioSource audioSource;
    public AudioClip buttonClick;
    public AudioClip[] typingSounds;
    public int soundVolume = 100;
    public int musicVolume = 100;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
    }
    // Then wherever you want to play a sound, you can call AudioManager.instance.PlayClip(soundEffect);
    public void PlayClip(AudioClip clip)
    {
        audioSource.volume = ((float)soundVolume) / 100f; 
        audioSource.PlayOneShot(clip);
    }
    public void PlayMusic(AudioClip clip)
    {
        audioSource.volume = ((float)musicVolume) / 100f; 
        audioSource.PlayOneShot(clip);
    }
    public void PlayRandomTypingSound()
    {
        AudioClip clip = typingSounds[Random.Range(0, typingSounds.Length)];
        audioSource.PlayOneShot(clip);
    }
    //Can use this to randomize pitch and volume of sound effects
    private void RandomizeSound()
    {
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = Random.Range(0.9f, 1.1f);
    }
}
