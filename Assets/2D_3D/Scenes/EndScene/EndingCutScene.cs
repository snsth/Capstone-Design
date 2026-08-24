using System.Collections;
using UnityEngine;

public class EndingCutscene : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform camTransform;
    [SerializeField] private CameraShake cameraShake;

    [Header("World")]
    [SerializeField] private GameObject backrooms;
    [SerializeField] private GameObject monitor;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;

    [Header("Camera Move")]
    [SerializeField] private Transform targetCamPoint;
    [SerializeField] private float moveDuration = 3f;

    private void Start()
    {
        StartCoroutine(PlayEnding());
    }

    private IEnumerator PlayEnding()
    {
        // ======================
        // 1. 시작: 검은 화면
        // ======================
        SetFade(1f);

        backrooms.SetActive(false);

        // ✔ 모니터 처음부터 켜기
        monitor.SetActive(true);

        yield return new WaitForSeconds(1.5f);

        // ======================
        // 2. 백룸 등장
        // ======================
        backrooms.SetActive(true);

        yield return new WaitForSeconds(1f);

        // ======================
        // 3. 화면 등장 (검은 → 보임)
        // ======================
        yield return StartCoroutine(Fade(1f, 0f));

        // ======================
        // 4. 카메라 흔들림 시작
        // ======================
        cameraShake.StartShake();

        // ======================
        // 5. 카메라 이동 (지정 위치로)
        // ======================
        yield return StartCoroutine(MoveCamera());

        yield return new WaitForSeconds(2f);

        // ======================
        // 6. 엔딩 종료 페이드 (보임 → 검정)
        // ======================
        yield return StartCoroutine(Fade(0f, 1f));

        cameraShake.StopShake();

        QuitGame();
    }

    // ======================
    // Camera Move (고정 목표 이동)
    // ======================
    private IEnumerator MoveCamera()
    {
        Vector3 start = camTransform.position;
        Vector3 end = targetCamPoint.position;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            camTransform.position = Vector3.Lerp(start, end, t);

            yield return null;
        }

        camTransform.position = end;
    }

    // ======================
    // Fade
    // ======================
    private IEnumerator Fade(float from, float to)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeGroup.alpha = to;
    }

    private void SetFade(float value)
    {
        fadeGroup.alpha = value;
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}