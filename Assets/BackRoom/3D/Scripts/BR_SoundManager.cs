using System.Collections;
using UnityEngine;

public class BR_SoundManager : MonoBehaviour
{
    public static BR_SoundManager Instance { get; private set; }

    [Header("발소리 (Footstep)")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float footstepVolume = 0.65f;

    [Header("물 진입 (Water Entry)")]
    public AudioClip waterEntryClip;
    [Range(0f, 1f)] public float waterEntryVolume = 0.85f;

    [Header("배경음 - 항상 재생 (Ambient)")]
    public AudioClip ambientClip;
    [Range(0f, 1f)] public float ambientVolume = 0.35f;

    [Header("형광등 지지직 (Fluorescent Buzz)")]
    public AudioClip fluorescentBuzzClip;
    [Range(0f, 1f)] public float fluorescentBuzzVolume = 0.5f;
    [Tooltip("다음 형광등 소리까지 최소 대기 시간(초)")]
    public float fluorescentMinInterval = 4f;
    [Tooltip("다음 형광등 소리까지 최대 대기 시간(초)")]
    public float fluorescentMaxInterval = 14f;

    AudioSource footstepSource;
    AudioSource sfxSource;
    AudioSource ambientSource;
    AudioSource fluorescentSource;
    bool inWater;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        footstepSource = AddSource(loop: false);
        sfxSource = AddSource(loop: false);
        ambientSource = AddSource(loop: true);
        fluorescentSource = AddSource(loop: false);

        // 수중 사운드는 물 진입 시에만 시작 — 기본은 정지
        ambientSource.clip = ambientClip;
        ambientSource.volume = ambientVolume;

        StartCoroutine(FluorescentLoop());
    }

    AudioSource AddSource(bool loop)
    {
        var src = gameObject.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.spatialBlend = 0f;
        return src;
    }

    // 발소리: 수중에서는 재생 안 함, Play()로 중첩 방지
    public void PlayFootstep()
    {
        if (inWater) return;
        if (footstepClips == null || footstepClips.Length == 0) return;
        footstepSource.clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepSource.volume = footstepVolume;
        footstepSource.Play();
    }

    // 물 진입 소리: OnTriggerEnter에서 첫 물 접촉 시 호출
    public void PlayWaterEntry()
    {
        if (waterEntryClip == null) return;
        sfxSource.PlayOneShot(waterEntryClip, waterEntryVolume);
    }

    // 수중 여부에 따라 수중 사운드 켜기/끄기 + 발소리 차단
    public void SetNearWater(bool isNear)
    {
        if (inWater == isNear) return;
        inWater = isNear;

        if (isNear)
        {
            footstepSource.Stop();
            if (ambientClip != null) ambientSource.Play();
        }
        else
        {
            ambientSource.Stop();
        }
    }

    // 형광등: 랜덤 간격으로 1~3회 짧게 지지직
    IEnumerator FluorescentLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(fluorescentMinInterval, fluorescentMaxInterval));

            if (fluorescentBuzzClip == null) continue;

            int bursts = Random.Range(1, 4);
            for (int i = 0; i < bursts; i++)
            {
                fluorescentSource.PlayOneShot(fluorescentBuzzClip, fluorescentBuzzVolume);
                if (i < bursts - 1)
                    yield return new WaitForSeconds(Random.Range(0.04f, 0.18f));
            }
        }
    }
}
