using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("#BGM")]
    public AudioClip bgmClip;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmFilter;

    [Header("#SFX")]
    public AudioClip[] sfxClips;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public int channels = 8;
    AudioSource[] sfxPlayers;
    int channelIndex;

    public enum SFX { Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win }

    void Awake()
    {
        // 씬 전환 시 중복 방지
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }

    void Init()
    {
        // BGM 플레이어 초기화
        GameObject bgmObj = new GameObject("BGM Player");
        bgmObj.transform.parent = transform;
        bgmPlayer = bgmObj.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;
        bgmPlayer.loop = true;
        bgmPlayer.clip = bgmClip;
        if (bgmGroup != null)
            bgmPlayer.outputAudioMixerGroup = bgmGroup;

        // SFX 플레이어 초기화
        GameObject sfxObj = new GameObject("SFX Player");
        sfxObj.transform.parent = transform;
        sfxPlayers = new AudioSource[channels];
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = sfxObj.AddComponent<AudioSource>();
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].bypassListenerEffects = true;
            if (sfxGroup != null)
                sfxPlayers[i].outputAudioMixerGroup = sfxGroup;
        }

        // Camera의 HighPassFilter (없을 수 있으므로 null 허용)
        if (Camera.main != null)
            bgmFilter = Camera.main.GetComponent<AudioHighPassFilter>();

        // 저장된 볼륨 불러오기 (없으면 기본값 사용)
        SetBgmVolume(PlayerPrefs.GetFloat("BGMVolume", bgmVolume));
        SetSfxVolume(PlayerPrefs.GetFloat("SFXVolume", sfxVolume));
    }

    // ── 볼륨 제어 ──────────────────────────────────────────

    // 0~1 값을 dB로 변환해 Mixer에 적용하고 PlayerPrefs에 저장
    public void SetBgmVolume(float volume)
    {
        bgmVolume = volume;
        if (audioMixer != null)
            audioMixer.SetFloat("BGMVolume", LinearToDb(volume));
        else if (bgmPlayer != null)
            bgmPlayer.volume = volume;
        PlayerPrefs.SetFloat("BGMVolume", volume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = volume;
        if (audioMixer != null)
            audioMixer.SetFloat("SFXVolume", LinearToDb(volume));
        else
        {
            foreach (var p in sfxPlayers)
                if (p != null) p.volume = volume;
        }
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    // 씬 전환 시 BGM 교체
    public void ChangeBgm(AudioClip newClip)
    {
        if (bgmPlayer.clip == newClip) return;
        bgmPlayer.Stop();
        bgmPlayer.clip = newClip;
        bgmPlayer.Play();
    }

    // ── 기존 인터페이스 ────────────────────────────────────

    public void PlayBgm(bool isPlay)
    {
        if (isPlay) bgmPlayer.Play();
        else bgmPlayer.Stop();
    }

    public void EffectBgm(bool isPlay)
    {
        // 씬마다 카메라가 달라지므로 매번 탐색
        if (bgmFilter == null && Camera.main != null)
            bgmFilter = Camera.main.GetComponent<AudioHighPassFilter>();
        if (bgmFilter != null)
            bgmFilter.enabled = isPlay;
    }

    public void PlaySfx(SFX sfx)
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            int loopIndex = (i + channelIndex) % sfxPlayers.Length;
            if (sfxPlayers[loopIndex].isPlaying) continue;

            int randomIndex = 0;
            if (sfx == SFX.Hit || sfx == SFX.Melee)
                randomIndex = Random.Range(0, 2);

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + randomIndex];
            sfxPlayers[loopIndex].Play();
            break;
        }
    }

    // ── 유틸 ───────────────────────────────────────────────

    // 선형 볼륨(0~1) → dB 변환 (0에 가까울 때 -80dB로 음소거)
    float LinearToDb(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}
