using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System.Text;
using TMPro;
using UnityEngine.UI;
using Photon.Pun.UtilityScripts;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public List<AudioSource> MusicSources;
    public List<AudioSource> SoundSources;
    public Slider MusicSlider;
    public Slider SoundSlider;

    // Start is called before the first frame update
    void Start()
    {
        this.gameObject.SetActive(false);

        foreach (GameObject source in GameObject.FindGameObjectsWithTag("MusicSource"))
        { 
            MusicSources.Add(source.GetComponent<AudioSource>());
        }
        foreach (GameObject source in GameObject.FindGameObjectsWithTag("SoundSource"))
        {
            SoundSources.Add(source.GetComponent<AudioSource>());
        }

        UpdateAudioVolumes();
        UpdateSliderValues();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAudioVolumes()
    {
        PlayerPrefs.SetFloat("SoundVolume", SoundSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", MusicSlider.value);

        UpdateAudioVolumes();
    }

    void UpdateAudioVolumes()
    {
        try
        {
            if (MusicSources != null)
            {
                foreach (AudioSource source in MusicSources)
                {
                    //Set audio source to player pref saved level
                    source.volume = PlayerPrefs.GetFloat("MusicVolume");
                }
            }

            if (SoundSources != null)
            {
                foreach (AudioSource source in SoundSources)
                {
                    //Set audio source to player pref saved level
                    source.volume = PlayerPrefs.GetFloat("SoundVolume");
                }
            }
        }

        catch
        {
            Debug.Log("No audio values in player prefs, creating them!");
            SetAudioVolumes();
        }
    }

    void UpdateSliderValues()
    {
       MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
       SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
