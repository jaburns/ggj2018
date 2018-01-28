using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;


public class AudioControl : MonoBehaviour {

    public AudioMixer masterMixer;
    public string masterVolume = "MasterVolume";
    public string musicVolume = "MusicVolume";
    public string SFXVolume = "SFXVolume";

    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;


    public void loadExisting()
    {

        float existingMaster = PlayerPrefs.GetFloat(masterVolume, 1f);
        float existingMusic = PlayerPrefs.GetFloat(musicVolume, 1f);
        float existingSFX = PlayerPrefs.GetFloat(SFXVolume, 1f);


        SetMasterLevel(existingMaster);
        SetMusicLevel(existingMusic);
        SetSfxLevel(existingSFX);

        masterSlider.value = existingMaster;
        musicSlider.value = existingMusic;
        sfxSlider.value = existingSFX;
    }

    private void OnEnable()
    {
        loadExisting();

    }

    private void OnDisable()
    {
        float newMaster;
        float newMusic;
        float newSFX;

        masterMixer.GetFloat(masterVolume, out newMaster);
        masterMixer.GetFloat(musicVolume, out newMusic);
        masterMixer.GetFloat(SFXVolume, out newSFX);

        newMaster = convertDBToFLoat(newMaster);
        newMusic = convertDBToFLoat(newMusic);
        newSFX = convertDBToFLoat(newSFX);

        PlayerPrefs.SetFloat(masterVolume, newMaster);
        PlayerPrefs.SetFloat(musicVolume, newMusic);
        PlayerPrefs.SetFloat(SFXVolume, newSFX);
    }



    /*convert d8 to float
     * 10^(x/20)-y
     */
    float convertDBToFLoat(float dB)
    {
        float volume = 0;

        //x/20

        //volume = x/20
        volume = dB / 20;
        // volume = 10 ^ volume
        volume = Mathf.Pow(10f, volume);


        return volume;

    }

    float convertFloatToDB(float volume)
    {
        float dB = 20 * Mathf.Log10(volume); ;


        return dB;
    }

    // Ensure each variable eg. "masterVol" is named EXACTLY right to what you exposed the variable names as.
    public void SetMasterLevel(float MasterLevel)
    {
        MasterLevel = convertFloatToDB(MasterLevel);
        masterMixer.SetFloat("MasterVolume", MasterLevel);
    }

    public void SetSfxLevel(float sfxLevel)
    {
        sfxLevel = convertFloatToDB(sfxLevel);
        masterMixer.SetFloat("SFXVolume", sfxLevel);
    }

    public void SetMusicLevel(float musicLevel)
    {
        musicLevel = convertFloatToDB(musicLevel);
        masterMixer.SetFloat("MusicVolume", musicLevel);
    }

}

