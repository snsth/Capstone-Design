using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class BR_PostProcessing : MonoBehaviour
{
    public static BR_PostProcessing Instance { get; private set; }

    [Header("기본 분위기")]
    [Range(0f, 1f)] public float baseVignetteIntensity  = 0.10f;
    [Range(0f, 1f)] public float baseFilmGrainIntensity = 0.08f;
    public float baseContrast   = 8f;
    public float basePostExposure = -0.2f;

    [Header("죽음 연출")]
    [Range(0f, 1f)] public float deathVignetteIntensity = 0.97f;
    [Range(0f, 1f)] public float deathChromatic         = 1f;
    [Range(-1f, 1f)] public float deathLensDistortion   = -0.45f;
    [Range(0f, 1f)] public float deathFilmGrain         = 0.65f;
    public Color deathColorFilter = new Color(0.72f, 0.07f, 0.04f);
    public float deathPostExposure = -1.8f;

    Volume volume;

    Vignette             vignette;
    ChromaticAberration  chromatic;
    ColorAdjustments     colorAdj;
    FilmGrain            filmGrain;
    LensDistortion       lensDist;

    Coroutine deathRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildVolume();
    }

    void BuildVolume()
    {
        var go = new GameObject("[BR_PostProcessVolume]");
        DontDestroyOnLoad(go);

        volume          = go.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.priority = 100;
        volume.profile  = ScriptableObject.CreateInstance<VolumeProfile>();

        vignette  = volume.profile.Add<Vignette>(true);
        chromatic = volume.profile.Add<ChromaticAberration>(true);
        colorAdj  = volume.profile.Add<ColorAdjustments>(true);
        filmGrain = volume.profile.Add<FilmGrain>(true);
        lensDist  = volume.profile.Add<LensDistortion>(true);

        ApplyBase();
    }

    void ApplyBase()
    {
        // 비네트: 어두운 가장자리
        vignette.intensity.Override(baseVignetteIntensity);
        vignette.smoothness.Override(0.45f);
        vignette.rounded.Override(true);

        // 컬러 보정: 전체적으로 어둡고 대비 높게
        colorAdj.contrast.Override(baseContrast);
        colorAdj.postExposure.Override(basePostExposure);
        colorAdj.colorFilter.Override(new Color(0.88f, 0.88f, 1f)); // 약간 차가운 느낌

        // 필름 그레인: 미세한 노이즈
        filmGrain.intensity.Override(baseFilmGrainIntensity);
        filmGrain.type.Override(FilmGrainLookup.Thin1);
        filmGrain.response.Override(0.8f);

        // 색수차 / 렌즈 왜곡 비활성
        chromatic.intensity.Override(0f);
        lensDist.intensity.Override(0f);
    }

    // ─── 공개 API ────────────────────────────────────────────────

    public void TriggerDeathEffect()
    {
        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(DeathRoutine());
    }

    public void ResetToBase() => ApplyBase();

    // ─── 코루틴 ──────────────────────────────────────────────────

    IEnumerator DeathRoutine()
    {
        // ── 0단계: 0.05 초 — 흰 섬광 (순간 충격) ─────────────────
        colorAdj.postExposure.Override(2.8f);
        colorAdj.colorFilter.Override(Color.white);
        vignette.intensity.Override(0.05f);
        chromatic.intensity.Override(deathChromatic);
        lensDist.intensity.Override(deathLensDistortion * 0.6f);
        filmGrain.intensity.Override(deathFilmGrain);

        yield return new WaitForSeconds(0.05f);

        // ── 1단계: 0.25 초 — 섬광 → 핏빛 어둠으로 전환 ──────────
        float dur = 0.25f, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            colorAdj.postExposure.Override(Mathf.Lerp(2.8f, deathPostExposure, p));
            colorAdj.colorFilter.Override(Color.Lerp(Color.white, deathColorFilter, p));
            vignette.intensity.Override(Mathf.Lerp(0.05f, deathVignetteIntensity, p));
            lensDist.intensity.Override(Mathf.Lerp(deathLensDistortion * 0.6f, deathLensDistortion, p));
            yield return null;
        }

        // ── 2단계: 1.5 초 — 클로즈업 유지, 색수차·왜곡 서서히 복귀 ──
        dur = 1.5f; t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            chromatic.intensity.Override(Mathf.Lerp(deathChromatic, 0.15f, p));
            lensDist.intensity.Override(Mathf.Lerp(deathLensDistortion, 0f, p));
            // 비네트·컬러는 공포 분위기 유지
            yield return null;
        }
    }
}
