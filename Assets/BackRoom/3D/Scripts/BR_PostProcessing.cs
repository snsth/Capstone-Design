using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class BR_PostProcessing : MonoBehaviour
{
    public static BR_PostProcessing Instance { get; private set; }

    [Header("기본 분위기")]
    [Range(0f, 1f)] public float baseVignetteIntensity  = 0f;
    [Range(0f, 1f)] public float baseFilmGrainIntensity = 0.08f;
    public float baseContrast   = 8f;
    public float basePostExposure = -0.2f;

    [Header("죽음 연출")]
    [Range(0f, 1f)] public float deathVignetteIntensity = 0f;
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
        vignette.active  = false;
        colorAdj.active  = false;
        filmGrain.active = false;
        chromatic.active = false;
        lensDist.active  = false;
    }

    // ─── 공개 API ────────────────────────────────────────────────

    public void TriggerDeathEffect()
    {
        if (deathRoutine != null) StopCoroutine(deathRoutine);
        deathRoutine = StartCoroutine(DeathRoutine());
    }

    public void ResetToBase() => ApplyBase();

    // 익사 — 서서히 어두워지며 씬 리로드
    public void TriggerDrowningEffect(float fadeDuration = 2.5f)
    {
        StartCoroutine(DrowningRoutine(fadeDuration));
    }

    // 산소량에 따라 화면 어둡게 (ratio 1=정상, 0=완전 암전)
    public void SetOxygenDim(float ratio)
    {
        if (ratio >= 1f)
        {
            colorAdj.active = false;
            return;
        }
        float t = 1f - ratio;
        colorAdj.active = true;
        colorAdj.postExposure.Override(Mathf.Lerp(0f, -5f, t));
        colorAdj.colorFilter.Override(Color.Lerp(Color.white, new Color(0.3f, 0.5f, 0.7f), t * 0.6f));
    }

    // ─── 코루틴 ──────────────────────────────────────────────────

    IEnumerator DrowningRoutine(float dur)
    {
        colorAdj.active = true;
        filmGrain.active = true;
        vignette.active = true;

        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            float p = t / dur;
            colorAdj.postExposure.Override(Mathf.Lerp(0f, -10f, p));
            colorAdj.colorFilter.Override(Color.Lerp(new Color(0.5f, 0.8f, 1f), Color.black, p));
            filmGrain.intensity.Override(Mathf.Lerp(0.1f, 0.5f, p));
            vignette.intensity.Override(Mathf.Lerp(0f, 0.9f, p));
            yield return null;
        }

        PlayerPrefs.SetInt("BR_HasDied", 1);
        PlayerPrefs.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    IEnumerator DeathRoutine()
    {
        // ── 0단계: 0.05 초 — 흰 섬광 (순간 충격) ─────────────────
        colorAdj.active  = true;
        chromatic.active = true;
        lensDist.active  = true;
        filmGrain.active = true;

        colorAdj.postExposure.Override(2.8f);
        colorAdj.colorFilter.Override(Color.white);
        chromatic.intensity.Override(deathChromatic);
        lensDist.intensity.Override(deathLensDistortion * 0.6f);
        filmGrain.intensity.Override(deathFilmGrain);
        filmGrain.type.Override(FilmGrainLookup.Thin1);

        yield return new WaitForSeconds(0.05f);

        // ── 1단계: 0.25 초 — 섬광 → 핏빛 어둠으로 전환 ──────────
        float dur = 0.25f, t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            colorAdj.postExposure.Override(Mathf.Lerp(2.8f, deathPostExposure, p));
            colorAdj.colorFilter.Override(Color.Lerp(Color.white, deathColorFilter, p));
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
