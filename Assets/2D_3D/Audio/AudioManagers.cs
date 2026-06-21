using UnityEngine;
using UnityEngine.Audio;

public class AudioManagers : MonoBehaviour
{
    [SerializeField]
    private AudioMixer mixer;

    public void SetBGMVolume(float volume)
    {
        mixer.SetFloat("BGMVolume", volume);
    }
}