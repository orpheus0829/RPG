using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundCtrl : MonoBehaviour
{
    public Slider soundSlider;
    public AudioSource playerAudio;

    private const string soundVolumeKey = "GameSoundVolume";

    public void Awake()
    {
        soundSlider = GetComponent<Slider>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerAudio = playerObj.GetComponent<AudioSource>();
        }
        soundSlider.onValueChanged.RemoveAllListeners();
        soundSlider.onValueChanged.AddListener(SetVolume);
    }

    public void OnEnable()
    {
        float saveVolume = PlayerPrefs.GetFloat(soundVolumeKey, 0.7f);
        soundSlider.value = saveVolume;
        SetVolume(saveVolume);
    }

    public void OnDisable()
    {

    }
    private void SetVolume(float value)
{
    playerAudio.volume = value;
    PlayerPrefs.SetFloat("GameSoundVolume", value);
    PlayerPrefs.Save();
    Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    if (player != null)
    {
        player.RefreshAudioVolume();
    }
}
    public void SwitchSound()
    {
        if (playerAudio == null)
        {
            return;
        }
        playerAudio.mute = !playerAudio.mute;
    }
}