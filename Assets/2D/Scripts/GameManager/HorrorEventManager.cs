using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 2D 게임 내 공포 이벤트를 시간 순서대로 발동시키는 매니저.
///
/// [이벤트 타임라인 — 120초 기준]
///  35s  - 팬텀 스폰:  플레이어 주변에 몬스터가 나타났다 사라짐
///  40s  - 정적 노이즈: CRT/전기음 재생 + BGM HighPassFilter 왜곡
///  60s  - 적 동결:    모든 적이 3초간 멈춤 → 갑자기 재개
///  75s  - 점프스케어: 빨간 화면 번쩍 + 큰 소리
///  90s  - 대규모 스폰: 적 대량 등장 + 플레이어 속도 부스트 + BGM 전환
/// 105s  - 노이즈+동결 콤보
/// 118s  - 강제 사망: 큰 소리 + 화면 암전 → GameOver (→ 3D 전환 트리거)
/// </summary>
public class HorrorEventManager : MonoBehaviour
{
    public static HorrorEventManager instance;

    [Header("# 이벤트 발동 시각 (초) — 총 120초 기준")]
    public float timeStaticNoise    = 40f;
    public float timeEnemyFreeze    = 60f;
    public float timeJumpscare      = 75f;
    public float timeMassSpawn      = 90f;
    public float timeNoiseAndFreeze = 105f;
    public float timeSuddenDeath    = 118f;

    [Header("# 대규모 스폰 설정")]
    public int   horrorSpawnCount       = 30;
    public float horrorSpawnSpeed       = 4f;
    public float speedBoostMultiplier   = 2.5f;  // 이동속도 배율
    public float speedBoostDuration     = 8f;    // 속도 증가 지속 시간(초)

    [Header("# 팬텀 스폰 설정")]
    public float timePhantomSpawn   = 35f;  // 발동 시각 (Inspector에서 조정 가능)
    public int   phantomSpawnCount  = 25;
    public float phantomSpawnRadius = 5f;

    [Header("# 화면 연출")]
    public Image screenOverlay;   // Canvas에 배치한 전체화면 Image (기본 alpha=0)

    Spawner spawner;

    // 각 이벤트 발동 여부 추적
    bool firedPhantom, firedStatic, firedFreeze, firedJumpscare, firedMass, firedCombo, firedDeath;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        spawner = FindObjectOfType<Spawner>();

        if (screenOverlay != null)
            screenOverlay.color = Color.clear;
    }

    void Update()
    {
        if (!Gamemanager.instance.isLive) return;

        float t = Gamemanager.instance.gameTime;

        if (!firedPhantom   && t >= timePhantomSpawn)   { firedPhantom   = true; StartCoroutine(Event_PhantomSpawn()); }
        if (!firedStatic    && t >= timeStaticNoise)    { firedStatic    = true; StartCoroutine(Event_StaticNoise()); }
        if (!firedFreeze    && t >= timeEnemyFreeze)    { firedFreeze    = true; StartCoroutine(Event_EnemyFreeze(3f)); }
        if (!firedJumpscare && t >= timeJumpscare)      { firedJumpscare = true; StartCoroutine(Event_Jumpscare()); }
        if (!firedMass      && t >= timeMassSpawn)      { firedMass      = true; StartCoroutine(Event_MassSpawn()); }
        if (!firedCombo     && t >= timeNoiseAndFreeze) { firedCombo     = true; StartCoroutine(Event_NoiseAndFreeze()); }
        if (!firedDeath     && t >= timeSuddenDeath)    { firedDeath     = true; StartCoroutine(Event_SuddenDeath()); }
    }

    // ── 공포 이벤트 0: 팬텀 스폰 ──────────────────────────────────────
    // 플레이어 주변에 몬스터가 순간 나타났다가 1~2초 후 사라짐
    IEnumerator Event_PhantomSpawn()
    {
        AudioManager.instance.StartBgmDuck(3f);
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorPhantomSpawn);

        // 전체 적 동결 + 팬텀 소환
        EnemyMovement.isFrozen = true;
        List<GameObject> phantoms = spawner?.SpawnPhantomWave(phantomSpawnCount, phantomSpawnRadius);

        // 1~2초간 정지 상태 유지
        yield return new WaitForSeconds(Random.Range(1f, 2f));

        // 팬텀 전부 비활성화 (풀로 반환)
        if (phantoms != null)
            foreach (var e in phantoms)
                if (e != null && e.activeSelf) e.SetActive(false);

        EnemyMovement.isFrozen = false;
    }

    // ── 공포 이벤트 1: 정적 노이즈 ─────────────────────────────────────
    // 전기/CRT 노이즈음 재생 + BGM HighPassFilter로 음질 왜곡
    IEnumerator Event_StaticNoise()
    {
        AudioManager.instance.StartBgmDuck(5f);         // BGM 급격히 낮춤
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorStatic);
        AudioManager.instance.EffectBgm(true);          // HighPassFilter ON

        yield return new WaitForSeconds(4f);

        AudioManager.instance.EffectBgm(false);         // 복구
    }

    // ── 공포 이벤트 2: 적 전체 동결 ────────────────────────────────────
    // 모든 적이 일제히 멈추고 BGM이 조용해졌다가, 갑자기 소리와 함께 재개
    IEnumerator Event_EnemyFreeze(float duration)
    {
        EnemyMovement.isFrozen = true;

        // BGM 낮춤 (freeze 전체 구간 + 노이즈 재생 직후까지)
        AudioManager.instance.StartBgmDuck(duration + 1.5f);

        // 화면 살짝 어둡게
        yield return StartCoroutine(FadeOverlay(new Color(0f, 0f, 0f, 0.35f), 0.5f));
        yield return new WaitForSeconds(duration - 1f);
        yield return StartCoroutine(FadeOverlay(Color.clear, 0.5f));

        // 갑자기 재개 — 노이즈 소리로 공포감 강조
        EnemyMovement.isFrozen = false;
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorNoise);
    }

    // ── 공포 이벤트 3: 점프스케어 ──────────────────────────────────────
    // 빨간 화면 번쩍임 + 큰 소리
    IEnumerator Event_Jumpscare()
    {
        AudioManager.instance.StartBgmDuck(2f);         // BGM 급격히 낮춤
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorJumpscare);

        // 빠른 빨간 번쩍임 3회
        for (int i = 0; i < 3; i++)
        {
            float alpha = (i == 0) ? 0.8f : 0.4f;
            if (screenOverlay != null)
                screenOverlay.color = new Color(1f, 0f, 0f, alpha);
            yield return new WaitForSeconds(0.07f);
            if (screenOverlay != null)
                screenOverlay.color = Color.clear;
            yield return new WaitForSeconds(0.05f);
        }
    }

    // ── 공포 이벤트 4: 대규모 스폰 ─────────────────────────────────────
    // 적 수십 마리 등장 + 플레이어 이동속도 부스트 + BGM 전환
    IEnumerator Event_MassSpawn()
    {
        AudioManager.instance.StartBgmDuck(3f);
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorMassSpawn);
        spawner?.SpawnHorrorWave(horrorSpawnCount, horrorSpawnSpeed);

        // 덕킹 무음 구간이 끝나는 시점에 BGM 교체 예약 → fade-in 시 새 BGM이 들림
        AudioManager.instance.QueueBgmChange(AudioManager.instance.bgm_Tension);

        // 속도 부스트는 별도 코루틴 (이 이벤트 흐름을 블록하지 않음)
        StartCoroutine(SpeedBoostRoutine());

        yield return null;
    }

    // 플레이어 속도를 일시적으로 증가 후 원래대로 복구
    IEnumerator SpeedBoostRoutine()
    {
        PlayerController player = Gamemanager.instance.player;
        float original = player.speed;
        player.speed = original * speedBoostMultiplier;

        yield return new WaitForSeconds(speedBoostDuration);

        if (player != null)
            player.speed = original;
    }

    // ── 공포 이벤트 5: 노이즈 + 동결 콤보 ─────────────────────────────
    IEnumerator Event_NoiseAndFreeze()
    {
        yield return StartCoroutine(Event_StaticNoise());
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(Event_EnemyFreeze(4f));
        // 동결 해제 직후 대규모 스폰
        yield return StartCoroutine(Event_MassSpawn());
    }

    // ── 공포 이벤트 6: 강제 사망 ───────────────────────────────────────
    // 큰 소리 + 화면 암전 후 게임오버 → 3D 씬 전환 트리거
    IEnumerator Event_SuddenDeath()
    {
        // BGM 즉시 소멸 (게임 종료이므로 복구 불필요 — 충분히 긴 holdDuration 사용)
        AudioManager.instance.StartBgmDuck(99f);
        // 큰 소리
        AudioManager.instance.PlaySfx(AudioManager.SFX.HorrorJumpscare);

        // 화면 빠르게 새빨개졌다가 암전
        if (screenOverlay != null)
            screenOverlay.color = new Color(1f, 0f, 0f, 0.9f);
        yield return new WaitForSeconds(0.2f);

        yield return StartCoroutine(FadeOverlay(Color.black, 1.5f));

        // 체력 강제 소진 → GameOver
        // GameOver 내부에서 Lose 화면이 뜨며, 그곳에서 3D 씬 로드로 연결 가능
        Gamemanager.instance.health = 0;
        Gamemanager.instance.GameOver();
    }

    // ── 유틸: 오버레이 페이드 ──────────────────────────────────────────
    IEnumerator FadeOverlay(Color target, float duration)
    {
        if (screenOverlay == null) yield break;

        Color start = screenOverlay.color;
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
