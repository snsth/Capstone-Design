using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Inspector 드롭다운에서 이벤트 종류 선택
public enum HorrorEventType
{
    PhantomSpawn,    // 팬텀 스폰
    StaticNoise,     // 정적 노이즈
    EnemyFreeze,     // 적 전체 동결
    PatternBreak,    // 이상행동
    Jumpscare,       // 점프스케어 (색상·피해% 자유 설정)
    MassSpawn,       // 대규모 스폰
    NoiseAndFreeze,  // 정적 노이즈 + 동결 콤보
    SuddenDeath,     // 강제 사망 (게임 오버)
    InputReverse,    // 이동 방향 반전
    InputBlock       // 이동 완전 차단
}

[System.Serializable]
public class HorrorEventData
{
    [Tooltip("Inspector에서 이벤트를 구분하는 이름")]
    public string label = "새 이벤트";
    public HorrorEventType type;
    [Tooltip("발동 시각 (초)")]
    public float triggerTime;

    [Header("── 점프스케어")]
    [Tooltip("화면 번쩍임 색상\n빨강=공포 / 흰색=섬광 / 보라=불안 / 초록=독")]
    public Color jumpscareColor = Color.red;
    [Range(0f, 0.5f), Tooltip("현재 체력의 몇 %를 깎을지 (0 = 피해 없음)")]
    public float damagePercent = 0.2f;

    [Header("── 팬텀 스폰")]
    public int   phantomCount  = 25;
    public float phantomRadius = 5f;

    [Header("── 동결 / 콤보")]
    public float freezeDuration = 3f;

    [Header("── 이상행동")]
    public int patternBreakCount = 3;

    [Header("── 대규모 스폰 / 콤보")]
    public int   massSpawnCount        = 30;
    public float massSpawnSpeed        = 4f;
    public float speedBoostMultiplier  = 2.5f;
    public float speedBoostDuration    = 8f;

    [Header("── 입력 조작")]
    [Tooltip("입력 반전 또는 차단 지속 시간 (초)")]
    public float inputDuration = 3f;

    [Header("── 정적 노이즈 반복")]
    [Tooltip("사운드를 몇 번 반복 재생할지")]
    public int   staticRepeatCount    = 4;
    [Tooltip("반복 재생 간격 (초) — 짧을수록 더 빠르게 끊기는 느낌")]
    public float staticRepeatInterval = 0.18f;
}

/// <summary>
/// 공포 이벤트 스케줄러.
/// Inspector의 Events 리스트에서 이벤트를 자유롭게 추가·제거·순서 변경 가능.
/// </summary>
public class HorrorEventManager : MonoBehaviour
{
    public static HorrorEventManager instance;

    [Header("# 공포 이벤트 목록 — 추가·제거·순서 변경 가능")]
    public List<HorrorEventData> events = new List<HorrorEventData>();

    [Header("# 화면 연출")]
    public Image screenOverlay;   // Canvas 전체화면 Image (alpha=0으로 시작)

    Spawner spawner;
    Weapon  axeWeapon;
    bool    isAxeHidden;
    bool[]  firedEvents;

    void Awake() { instance = this; }

    // Inspector에서 컴포넌트 우클릭 → "기본 이벤트 세팅 적용" 선택 시 자동 입력
    [ContextMenu("기본 이벤트 세팅 적용")]
    void SetDefaultEvents()
    {
        events = new List<HorrorEventData>
        {
            new HorrorEventData { label="팬텀 스폰",      type=HorrorEventType.PhantomSpawn,   triggerTime=35f,  phantomCount=25,  phantomRadius=5f },
            new HorrorEventData { label="정적 노이즈",    type=HorrorEventType.StaticNoise,    triggerTime=40f  },
            new HorrorEventData { label="적 동결",        type=HorrorEventType.EnemyFreeze,    triggerTime=60f,  freezeDuration=3f },
            new HorrorEventData { label="이상행동",       type=HorrorEventType.PatternBreak,   triggerTime=68f,  patternBreakCount=3 },
            new HorrorEventData { label="점프스케어",     type=HorrorEventType.Jumpscare,      triggerTime=75f,  jumpscareColor=Color.red, damagePercent=0.2f },
            new HorrorEventData { label="대규모 스폰",    type=HorrorEventType.MassSpawn,      triggerTime=90f,  massSpawnCount=30, massSpawnSpeed=4f, speedBoostMultiplier=2.5f, speedBoostDuration=8f },
            new HorrorEventData { label="노이즈+동결 콤보", type=HorrorEventType.NoiseAndFreeze, triggerTime=105f, freezeDuration=4f, massSpawnCount=20, massSpawnSpeed=4f, speedBoostMultiplier=2.5f, speedBoostDuration=8f },
            new HorrorEventData { label="강제 사망",      type=HorrorEventType.SuddenDeath,   triggerTime=118f },
        };
    }

    void Start()
    {
        spawner     = FindObjectOfType<Spawner>();
        firedEvents = new bool[events.Count];
        if (screenOverlay != null) screenOverlay.color = Color.clear;
    }

    void Update()
    {
        if (!Gamemanager.instance.isLive) return;

        float t = Gamemanager.instance.gameTime;

        for (int i = 0; i < firedEvents.Length; i++)
            if (!firedEvents[i] && t >= events[i].triggerTime)
            {
                firedEvents[i] = true;
                StartCoroutine(ExecuteEvent(events[i]));
            }

        if (!isAxeHidden) UpdateAxeFade(t);
    }

    IEnumerator ExecuteEvent(HorrorEventData data)
    {
        switch (data.type)
        {
            case HorrorEventType.PhantomSpawn:   yield return StartCoroutine(Event_PhantomSpawn(data));   break;
            case HorrorEventType.StaticNoise:    yield return StartCoroutine(Event_StaticNoise(data));     break;
            case HorrorEventType.EnemyFreeze:    yield return StartCoroutine(Event_EnemyFreeze(data.freezeDuration)); break;
            case HorrorEventType.PatternBreak:   yield return StartCoroutine(Event_PatternBreak(data));   break;
            case HorrorEventType.Jumpscare:      yield return StartCoroutine(Event_Jumpscare(data));       break;
            case HorrorEventType.MassSpawn:      yield return StartCoroutine(Event_MassSpawn(data));       break;
            case HorrorEventType.NoiseAndFreeze: yield return StartCoroutine(Event_NoiseAndFreeze(data)); break;
            case HorrorEventType.SuddenDeath:    yield return StartCoroutine(Event_SuddenDeath());         break;
            case HorrorEventType.InputReverse:   yield return StartCoroutine(Event_InputReverse(data));   break;
            case HorrorEventType.InputBlock:     yield return StartCoroutine(Event_InputBlock(data));     break;
        }
    }

    // ── 팬텀 스폰 ─────────────────────────────────────────────────────
    IEnumerator Event_PhantomSpawn(HorrorEventData data)
    {
        SetAxeHidden(true);
        AudioManager.instance.StartBgmDuck(3f);
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorPhantomSpawn);

        EnemyMovement.isFrozen = true;
        List<GameObject> phantoms = spawner?.SpawnPhantomWave(data.phantomCount, data.phantomRadius);

        yield return new WaitForSeconds(Random.Range(1f, 2f));

        if (phantoms != null)
            foreach (var e in phantoms)
                if (e != null && e.activeSelf) e.SetActive(false);

        EnemyMovement.isFrozen = false;
        SetAxeHidden(false);
    }

    // ── 정적 노이즈 ───────────────────────────────────────────────────
    IEnumerator Event_StaticNoise(HorrorEventData data)
    {
        AudioManager.instance.StartBgmDuck(5f);
        AudioManager.instance.EffectBgm(true);

        // 설정한 횟수만큼 간격을 두고 반복 재생 (PlayOneShot으로 겹쳐서 재생)
        for (int i = 0; i < data.staticRepeatCount; i++)
        {
            AudioManager.instance.PlaySfxOneShot(AudioManager.SFX.HorrorStatic);
            yield return new WaitForSeconds(data.staticRepeatInterval);
        }

        // 남은 시간 대기 후 필터 복구
        float played = data.staticRepeatInterval * data.staticRepeatCount;
        float remaining = Mathf.Max(0f, 4f - played);
        yield return new WaitForSeconds(remaining);

        AudioManager.instance.EffectBgm(false);
    }

    // ── 적 전체 동결 ──────────────────────────────────────────────────
    IEnumerator Event_EnemyFreeze(float duration)
    {
        EnemyMovement.isFrozen = true;
        AudioManager.instance.StartBgmDuck(duration + 1.5f);

        yield return StartCoroutine(FadeOverlay(new Color(0f, 0f, 0f, 0.35f), 0.5f));
        yield return new WaitForSeconds(duration - 1f);
        yield return StartCoroutine(FadeOverlay(Color.clear, 0.5f));

        EnemyMovement.isFrozen = false;
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorNoise);
        CameraEffect.instance?.ShakeOnly();
    }

    // ── 이상행동 ──────────────────────────────────────────────────────
    IEnumerator Event_PatternBreak(HorrorEventData data)
    {
        SetAxeHidden(true);

        EnemyMovement[] all = FindObjectsOfType<EnemyMovement>();
        List<EnemyMovement> active = new List<EnemyMovement>();
        foreach (var e in all)
            if (e.gameObject.activeSelf) active.Add(e);

        for (int i = active.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            EnemyMovement tmp = active[i]; active[i] = active[j]; active[j] = tmp;
        }

        int count = Mathf.Min(data.patternBreakCount, active.Count);
        for (int i = 0; i < count; i++)
            active[i].StartPatternBreak();

        yield return new WaitForSeconds(2.5f);
        SetAxeHidden(false);
    }

    // ── 점프스케어 ────────────────────────────────────────────────────
    // 기법1(완전무음) → 기법2(무기정지) → 1.5~2s →
    // 기법3(카메라펀치) + 충격음 + % 피해 + 색상 번쩍임
    IEnumerator Event_Jumpscare(HorrorEventData data)
    {
        AudioManager.instance.MuteAllAudio();
        Scanner.isDisabled = true;

        yield return new WaitForSeconds(Random.Range(1.5f, 2.0f));

        AudioManager.instance.UnmuteAllAudio();
        Scanner.isDisabled = false;
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorJumpscare);
        CameraEffect.instance?.PunchAndShake();

        // 현재 체력의 % 피해 (최소 체력 1 보장)
        if (data.damagePercent > 0f)
        {
            float dmg = Gamemanager.instance.health * data.damagePercent;
            Gamemanager.instance.health = Mathf.Max(1f, Gamemanager.instance.health - dmg);
        }

        // 색상 번쩍임 3회
        Color fc = data.jumpscareColor;
        for (int i = 0; i < 3; i++)
        {
            float alpha = (i == 0) ? 0.85f : 0.45f;
            if (screenOverlay != null)
                screenOverlay.color = new Color(fc.r, fc.g, fc.b, alpha);
            yield return new WaitForSeconds(0.07f);
            if (screenOverlay != null)
                screenOverlay.color = Color.clear;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // ── 대규모 스폰 ───────────────────────────────────────────────────
    IEnumerator Event_MassSpawn(HorrorEventData data)
    {
        SetAxeHidden(true);
        AudioManager.instance.StartBgmDuck(3f);
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorMassSpawn);
        spawner?.SpawnHorrorWave(data.massSpawnCount, data.massSpawnSpeed);
        AudioManager.instance.QueueBgmChange(AudioManager.instance.bgm_Tension);
        StartCoroutine(SpeedBoostRoutine(data.speedBoostMultiplier, data.speedBoostDuration));

        yield return new WaitForSeconds(3f);
        SetAxeHidden(false);
    }

    IEnumerator SpeedBoostRoutine(float multiplier, float duration)
    {
        PlayerController player = Gamemanager.instance.player;
        float original = player.speed;
        player.speed   = original * multiplier;
        yield return new WaitForSeconds(duration);
        if (player != null) player.speed = original;
    }

    // ── 노이즈 + 동결 콤보 ───────────────────────────────────────────
    IEnumerator Event_NoiseAndFreeze(HorrorEventData data)
    {
        yield return StartCoroutine(Event_StaticNoise(data));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Event_EnemyFreeze(data.freezeDuration));
        yield return StartCoroutine(Event_MassSpawn(data));
    }

    // ── 강제 사망 ─────────────────────────────────────────────────────
    IEnumerator Event_SuddenDeath()
    {
        Scanner.isDisabled = true;
        AudioManager.instance.MuteAllAudio();

        yield return new WaitForSeconds(0.8f);

        AudioManager.instance.UnmuteAllAudio();
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorJumpscare);
        CameraEffect.instance?.PunchAndShake();

        if (screenOverlay != null)
            screenOverlay.color = new Color(1f, 0f, 0f, 0.9f);
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeOverlay(Color.black, 1.5f));

        Scanner.isDisabled = false;
        Gamemanager.instance.GameOver();
    }

    // ── 입력 반전 ─────────────────────────────────────────────────────
    // WASD 방향이 뒤집혀 플레이어가 의도와 반대로 움직임
    IEnumerator Event_InputReverse(HorrorEventData data)
    {
        PlayerController.isInputReversed = true;

        // 반전 중임을 화면 색으로 살짝 표시 (보라빛 틴트)
        yield return StartCoroutine(FadeOverlay(new Color(0.4f, 0f, 0.6f, 0.25f), 0.3f));
        yield return new WaitForSeconds(data.inputDuration - 0.6f);
        yield return StartCoroutine(FadeOverlay(Color.clear, 0.3f));

        PlayerController.isInputReversed = false;
    }

    // ── 입력 차단 ─────────────────────────────────────────────────────
    // 플레이어가 완전히 움직이지 못하는 상태
    IEnumerator Event_InputBlock(HorrorEventData data)
    {
        PlayerController.isInputBlocked = true;
        AudioManager.instance.StartBgmDuck(data.inputDuration);

        // 차단 중임을 화면 어둡게 표시
        yield return StartCoroutine(FadeOverlay(new Color(0f, 0f, 0f, 0.5f), 0.2f));
        yield return new WaitForSeconds(data.inputDuration - 0.4f);
        yield return StartCoroutine(FadeOverlay(Color.clear, 0.2f));

        PlayerController.isInputBlocked = false;
    }

    // ── 도끼 무기 박탈 시스템 ─────────────────────────────────────────

    void UpdateAxeFade(float t)
    {
        if (axeWeapon == null)
        {
            foreach (var w in FindObjectsOfType<Weapon>())
                if (w.id == 0) { axeWeapon = w; break; }
            return;
        }
        float alpha = Mathf.Lerp(1f, 0.1f, t / Gamemanager.instance.maxGameTime);
        ApplyAxeAlpha(alpha);
    }

    void SetAxeHidden(bool hidden)
    {
        isAxeHidden = hidden;
        if (axeWeapon == null) return;
        foreach (Transform child in axeWeapon.transform)
        {
            SpriteRenderer sr  = child.GetComponent<SpriteRenderer>();
            Collider2D     col = child.GetComponent<Collider2D>();
            if (sr)  sr.enabled  = !hidden;
            if (col) col.enabled = !hidden;
        }
        if (!hidden)
        {
            float alpha = Mathf.Lerp(1f, 0.1f, Gamemanager.instance.gameTime / Gamemanager.instance.maxGameTime);
            ApplyAxeAlpha(alpha);
        }
    }

    void ApplyAxeAlpha(float alpha)
    {
        if (axeWeapon == null) return;
        foreach (Transform child in axeWeapon.transform)
        {
            SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
            if (sr) { Color c = sr.color; c.a = alpha; sr.color = c; }
        }
    }

    // ── 오버레이 페이드 ───────────────────────────────────────────────
    IEnumerator FadeOverlay(Color target, float duration)
    {
        if (screenOverlay == null) yield break;
        Color start   = screenOverlay.color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            screenOverlay.color = Color.Lerp(start, target, elapsed / duration);
            yield return null;
        }
        screenOverlay.color = target;
    }
}
