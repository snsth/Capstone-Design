using System.Collections;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    public static CameraEffect instance;

    [Header("# 펀치 설정")]
    public float punchAmount    = 0.8f;   // 줌인 강도 (orthographic size 감소량)
    public float shakeIntensity = 0.12f;  // 흔들림 강도
    public float shakeDuration  = 0.35f;  // 흔들림 지속 시간

    Camera cam;
    float baseOrthoSize;
    Vector2 shakeOffset;
    Coroutine _activeCoroutine;

    void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
        baseOrthoSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        // Follow/Player 이동 이후 흔들림 오프셋을 추가 — 드리프트 없이 매 프레임 적용
        if (shakeOffset != Vector2.zero)
            transform.position += (Vector3)shakeOffset;
    }

    // 카메라 펀치 (줌인 오버슈트) + 감쇠 흔들림
    // Jumpscare 등 충격 연출용
    public void PunchAndShake()
    {
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(PunchRoutine());
    }

    // 흔들림만 (줌 없이)
    // EnemyFreeze 해제, NoiseAndFreeze 콤보 해제용
    public void ShakeOnly(float duration = -1f, float intensity = -1f)
    {
        float d = duration  < 0 ? shakeDuration  * 0.6f : duration;
        float s = intensity < 0 ? shakeIntensity * 0.7f : intensity;
        if (_activeCoroutine != null) StopCoroutine(_activeCoroutine);
        _activeCoroutine = StartCoroutine(ShakeRoutine(d, s));
    }

    IEnumerator PunchRoutine()
    {
        // 1단계: 빠른 줌인 (0.05~0.08s)
        float punchInTime = Random.Range(0.05f, 0.08f);
        float zoomedSize  = baseOrthoSize - punchAmount;
        yield return StartCoroutine(TweenOrtho(baseOrthoSize, zoomedSize, punchInTime));

        // 2단계: 오버슈트 복귀 (살짝 더 커졌다 돌아옴)
        float overshootSize = baseOrthoSize + punchAmount * 0.25f;
        yield return StartCoroutine(TweenOrtho(zoomedSize, overshootSize, 0.06f));
        yield return StartCoroutine(TweenOrtho(overshootSize, baseOrthoSize, 0.1f));
        cam.orthographicSize = baseOrthoSize;

        // 3단계: 감쇠 흔들림
        yield return StartCoroutine(ShakeRoutine(shakeDuration, shakeIntensity));
    }

    IEnumerator TweenOrtho(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cam.orthographicSize = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cam.orthographicSize = to;
    }

    IEnumerator ShakeRoutine(float duration, float intensity)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = intensity * (1f - elapsed / duration); // 선형 감쇠
            shakeOffset = new Vector2(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength)
            );
            yield return null;
        }
        shakeOffset    = Vector2.zero;
        _activeCoroutine = null;
    }
}
