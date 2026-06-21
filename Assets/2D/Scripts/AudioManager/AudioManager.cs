using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;

    [Header("# BGM")]
    public AudioClip bgm_Normal;    // 기본 게임 BGM
    public AudioClip bgm_Tension;   // 대규모 스폰 이후 긴장감 BGM
    public AudioClip bgm_Horror;    // 추가 공포 BGM 슬롯
    [Range(0f, 1f)] public float bgmVolume = 1f;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmFilter;
    Coroutine _duckCoroutine;
    AudioClip _pendingBgmClip;   // 덕킹 종료 시 교체할 BGM 예약

    [Header("# SFX — 일반")]
    public AudioClip sfx_Dead;        // [0]  Dead     적 사망
    public AudioClip sfx_Hit1;        // [1]  Hit      피격 1
    public AudioClip sfx_Hit2;        // [2]  Hit      피격 2 (랜덤)
    public AudioClip sfx_LevelUp;     // [3]  LevelUp  레벨업
    public AudioClip sfx_Lose;        // [4]  Lose     패배
    public AudioClip sfx_Melee1;      // [5]  Melee    근접 1
    public AudioClip sfx_Melee2;      // [6]  Melee    근접 2 (랜덤)
    public AudioClip sfx_Range;       // [7]  Range    원거리
    public AudioClip sfx_Select;      // [8]  Select   선택/승리

    [Header("# SFX — 공포 이벤트")]
    public AudioClip sfx_HorrorNoise;        // [9]   HorrorNoise        전기/노이즈
    public AudioClip sfx_HorrorStatic;       // [10]  HorrorStatic       CRT 정적
    public AudioClip sfx_HorrorJumpscare;    // [11]  HorrorJumpscare    점프스케어
    public AudioClip sfx_HorrorPhantomSpawn; // [12]  HorrorPhantomSpawn 팬텀 소환
    public AudioClip sfx_HorrorMassSpawn;    // [13]  HorrorMassSpawn    대규모 스폰

    [Header("# SFX — 설정")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    public int channels = 8;
    AudioClip[] sfxClips;       // Init()에서 위 필드로 자동 조립
    AudioSource[] sfxPlayers;   // 일반 SFX 공유 채널 풀
    AudioSource horrorSfxPlayer; // 공포 SFX 전용 채널 (채널 경합 없이 항상 재생)
    int channelIndex;
    bool _isHorrorEventActive;

    public enum SFX
    {
        Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win,
        // 공포 이벤트용 SFX (sfxClips 배열 9·10·11 슬롯에 클립 할당)
        HorrorNoise = 9, HorrorStatic = 10, HorrorJumpscare = 11,
        HorrorPhantomSpawn = 12,  // 팬텀 스폰: 주위에 나타났다 사라지는 연출용
        HorrorMassSpawn = 13      // 대규모 스폰 전용 사운드
    }

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
        bgmPlayer.clip = bgm_Normal;
        if (bgmGroup != null)
            bgmPlayer.outputAudioMixerGroup = bgmGroup;

        // 일반 SFX 플레이어 초기화 (채널 풀)
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

        // 공포 SFX 전용 플레이어 (채널 경합 없이 항상 재생 보장)
        GameObject horrorObj = new GameObject("Horror SFX Player");
        horrorObj.transform.parent = transform;
        horrorSfxPlayer = horrorObj.AddComponent<AudioSource>();
        horrorSfxPlayer.playOnAwake = false;
        horrorSfxPlayer.bypassListenerEffects = true;
        if (sfxGroup != null)
            horrorSfxPlayer.outputAudioMixerGroup = sfxGroup;

        // Camera의 HighPassFilter (없을 수 있으므로 null 허용)
        if (Camera.main != null)
            bgmFilter = Camera.main.GetComponent<AudioHighPassFilter>();

        // SFX 배열 조립 (enum 인덱스 순서와 일치)
        sfxClips = new AudioClip[]
        {
            sfx_Dead,               // [0]  Dead
            sfx_Hit1,               // [1]  Hit
            sfx_Hit2,               // [2]  Hit (랜덤)
            sfx_LevelUp,            // [3]  LevelUp
            sfx_Lose,               // [4]  Lose
            sfx_Melee1,             // [5]  Melee
            sfx_Melee2,             // [6]  Melee (랜덤)
            sfx_Range,              // [7]  Range
            sfx_Select,             // [8]  Select / Win
            sfx_HorrorNoise,        // [9]  HorrorNoise
            sfx_HorrorStatic,       // [10] HorrorStatic
            sfx_HorrorJumpscare,    // [11] HorrorJumpscare
            sfx_HorrorPhantomSpawn, // [12] HorrorPhantomSpawn
            sfx_HorrorMassSpawn     // [13] HorrorMassSpawn
        };

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

    // 즉시 BGM 교체 (덕킹 없을 때 사용)
    public void ChangeBgm(AudioClip newClip)
    {
        if (newClip == null || bgmPlayer.clip == newClip) return;
        bgmPlayer.Stop();
        bgmPlayer.clip = newClip;
        bgmPlayer.Play();
    }

    // 덕킹 무음 구간이 끝나는 시점에 BGM 교체 예약
    // → 무음 상태에서 클립이 바뀌고 fade-in 시 새 BGM이 들림
    public void QueueBgmChange(AudioClip newClip)
    {
        _pendingBgmClip = newClip;
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
        // 공포 이벤트 중에는 일반 SFX 차단
        if (_isHorrorEventActive && (int)sfx < (int)SFX.HorrorNoise)
            return;

        // 공포 SFX → 전용 채널에서 재생 (채널 경합 없이 항상 재생 보장)
        if ((int)sfx >= (int)SFX.HorrorNoise)
        {
            horrorSfxPlayer.clip = sfxClips[(int)sfx];
            horrorSfxPlayer.Play();
            return;
        }

        // 일반 SFX → 공유 채널 풀에서 빈 채널 탐색
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

    // ── BGM 덕킹 (공포 이벤트용) ──────────────────────────

    // BGM 즉시 소멸 후 holdDuration 동안 유지, 이후 슬라이더 값으로 복구
    // bgmVolume(슬라이더)은 건드리지 않으므로 슬라이더 위치 그대로 유지됨
    public void StartBgmDuck(float holdDuration)
    {
        if (_duckCoroutine != null) StopCoroutine(_duckCoroutine);
        _duckCoroutine = StartCoroutine(BgmDuckRoutine(holdDuration));
    }

    // 공포 SFX 전용 채널 중지
    void StopHorrorSfx()
    {
        if (horrorSfxPlayer.isPlaying)
            horrorSfxPlayer.Stop();
    }

    IEnumerator BgmDuckRoutine(float holdDuration)
    {
        StopHorrorSfx();         // 이전 공포 SFX 먼저 중지
        _isHorrorEventActive = true;

        // BGM 즉시 0으로 (슬라이더 값 bgmVolume은 그대로)
        if (audioMixer != null)
            audioMixer.SetFloat("BGMVolume", -80f);
        else if (bgmPlayer != null)
            bgmPlayer.volume = 0f;

        yield return new WaitForSeconds(holdDuration);

        _isHorrorEventActive = false;

        // 예약된 BGM이 있으면 무음 상태에서 클립 교체 후 fade-in
        if (_pendingBgmClip != null)
        {
            bgmPlayer.clip = _pendingBgmClip;
            bgmPlayer.Play();
            _pendingBgmClip = null;
        }

        // 슬라이더 값(bgmVolume)으로 서서히 복구 (0.5초)
        yield return StartCoroutine(BgmFadeRoutine(bgmVolume, 0.5f));
        _duckCoroutine = null;
    }

    IEnumerator BgmFadeRoutine(float targetLinear, float duration)
    {
        if (audioMixer != null)
        {
            audioMixer.GetFloat("BGMVolume", out float startDb);
            float targetDb = LinearToDb(targetLinear);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                audioMixer.SetFloat("BGMVolume", Mathf.Lerp(startDb, targetDb, elapsed / duration));
                yield return null;
            }
            audioMixer.SetFloat("BGMVolume", targetDb);
        }
        else if (bgmPlayer != null)
        {
            float startVolume = bgmPlayer.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                bgmPlayer.volume = Mathf.Lerp(startVolume, targetLinear, elapsed / duration);
                yield return null;
            }
            bgmPlayer.volume = targetLinear;
        }
    }

    // ── 유틸 ───────────────────────────────────────────────

    // 선형 볼륨(0~1) → dB 변환 (0에 가까울 때 -80dB로 음소거)
    float LinearToDb(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}
