using UnityEngine;

public class SoundMgr : Base_mgr<SoundMgr>
{
    [Header("全局主音量")]
    [Range(0f, 1f)] public float globalVolume = 0.7f;
    private const string VolumeSaveKey = "GameGlobalVolume";

    protected override void Awake()
    {
        base.Awake();
        if (instance == this)
        {
            DontDestroyOnLoad(gameObject);
            globalVolume = PlayerPrefs.GetFloat(VolumeSaveKey, 0.7f);
            RefreshAllAudioSource();
        }
    }
    public void SetGlobalVolume(float value)
    {
        globalVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VolumeSaveKey, globalVolume);
        PlayerPrefs.Save();
        RefreshAllAudioSource();
    }
    public void RefreshAllAudioSource()
    {
        AudioSource[] allAudio = Object.FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in allAudio)
        {
            audio.volume = globalVolume;
        }
    }
    public void ToggleMuteAllAudio()
    {
        AudioSource[] allAudio = Object.FindObjectsOfType<AudioSource>();
        if (allAudio.Length == 0) return;

        bool targetMute = !allAudio[0].mute;
        foreach (AudioSource audio in allAudio)
        {
            audio.mute = targetMute;
        }
    }
    public void SyncSingleAudioSource(AudioSource audioSource)
    {
        if (audioSource != null)
        {
            audioSource.volume = globalVolume;
        }
    }
    public void PlaySingleSound(AudioClip clip,GameObject owner)
    {
        bool HaveAu = owner.TryGetComponent(out AudioSource au);
        if (!HaveAu)
        {
            au = owner.AddComponent<AudioSource>();
            SyncSingleAudioSource(au);
        }
        au.clip = clip;
        au.Play();
    }
}