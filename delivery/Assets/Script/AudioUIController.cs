using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioUIController : MonoBehaviour
{
    [Header("Mixer & Sliders")]
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // ����� �� �ҷ����� (������ 0.75)
        float bgmValue = PlayerPrefs.GetFloat("BGMVolume", 0.75f);
        float sfxValue = PlayerPrefs.GetFloat("SFXVolume", 0.75f);

        bgmSlider.value = bgmValue;
        sfxSlider.value = sfxValue;

        SetBGMVolume(bgmValue);
        SetSFXVolume(sfxValue);

        // �����̴� �� ���� �� ���� ����
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("BGMVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}
