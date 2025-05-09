using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Settings : MonoBehaviour
{
   
   public TextMeshProUGUI soundVolumeText; // UI Text element to display sound volume
   public TextMeshProUGUI musicVolumeText; // UI Text element to display music volume
   [SerializeField]
   private UnityEngine.UI.Slider soundVolumeSlider; // UI Slider for sound volume
   [SerializeField]
   private UnityEngine.UI.Slider musicVolumeSlider; // UI Slider for music volume
   void Start()
   {
       musicVolumeSlider.value = AudioManager.instance.musicVolume;
       soundVolumeSlider.value = AudioManager.instance.soundVolume;
       SetMusicVolume();
       SetSoundVolume();
   }

   

   public void SetSoundVolume()
   {
       
       AudioManager.instance.soundVolume = (int) soundVolumeSlider.value;
       soundVolumeText.text = "Sounds: <b><font-weight=900>"+ AudioManager.instance.soundVolume + "</font-weight></b>";
       
       
   }
   public void SetMusicVolume()
   {
       AudioManager.instance.musicVolume = (int) musicVolumeSlider.value;
       musicVolumeText.text = "Music: <b><font-weight=900>"+ AudioManager.instance.musicVolume + "</font-weight></b>";
       
       
   }
   public void ReturnToMenu()
   {
       AudioManager.instance.PlayClip(AudioManager.instance.buttonClick);
       SceneManager.LoadScene("MainMenu");
   }

   public void Save()
   {
       PlayerPrefs.SetInt("soundVolume", AudioManager.instance.soundVolume);
       PlayerPrefs.SetInt("musicVolume", AudioManager.instance.musicVolume);

       PlayerPrefs.Save();
   }
   
   
}
