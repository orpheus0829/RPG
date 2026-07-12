using UnityEngine;
using UnityEngine.UI;

public class SoundCtrl : MonoBehaviour
{
    public Slider soundSlider;
    public void Awake()
    {
        soundSlider = GetComponent<Slider>();
        soundSlider.onValueChanged.RemoveAllListeners();
        soundSlider.onValueChanged.AddListener(OnSliderValueChange);
    }
    public void OnEnable()
    {
        soundSlider.value = SoundMgr.instance.globalVolume;
    }
    private void OnSliderValueChange(float value)
    {
        SoundMgr.instance.SetGlobalVolume(value);
    }
    public void SwitchSound()
    {
        SoundMgr.instance.ToggleMuteAllAudio();
    }
    public void Update()
    {
        transform.parent.localScale = transform.parent.localScale == Vector3.one ? transform.parent.localScale : Vector3.one;
    }
}