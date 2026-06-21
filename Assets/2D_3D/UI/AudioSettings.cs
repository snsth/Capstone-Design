using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;

    public void SetBGMVolume(float value)
    {
        if (value <= 0f)
        {
            mixer.SetFloat("BGMVolume", -80f);
            return;
        }

        mixer.SetFloat(
            "BGMVolume",
            Mathf.Log10(value) * 20f
        );
    }

    public void SetSFXVolume(float value)
    {
        if (value <= 0f)
        {
            mixer.SetFloat("SFXVolume", -80f);
            return;
        }

        mixer.SetFloat(
            "SFXVolume",
            Mathf.Log10(value) * 20f
        );
    }
}