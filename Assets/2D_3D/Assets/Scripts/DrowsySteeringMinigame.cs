using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DrowsySteeringMinigame : MonoBehaviour
{
    [Header("References")]
    public Camera cam;                      // 비우면 Camera.main
    public Transform rollTarget;            // 보통 Main Camera
    public Image blackOverlay;              // 화면 어둡게(투명→검정 페이드)
    public GameObject uiRoot;               // MinigameUIRoot(선택)
    public Slider progressSlider;           // 진행도(0~1)
    public TMP_Text messageText;            // 시작/안내 문구
    public CarLocalFirstPersonLook lookRef; // 선택

    [Header("Message")]
    [TextArea] public string startMessage = "졸음이 쏟아집니다...";
    public float msgFadeIn = 0.25f;
    public float msgHold = -1f;             // -1 = 성공 때까지 유지
    public float msgFadeOut = 0.3f;

    public enum ClickMode { OnWheel, Anywhere }
    [Header("Click")]
    public ClickMode clickMode = ClickMode.OnWheel;
    public Collider steeringWheelCollider;  // OnWheel에서만 사용
    public LayerMask wheelMask = ~0;

    [Header("Gameplay")]
    public bool autoStartOnPlay = false;    // 트리거로만 시작(강제 false 처리)
    public float startDelay = 0f;

    public bool useTimeLimit = false;
    public float timeLimit = 4f;

    public float progressPerClick = 0.2f;
    public float progressDecayPerSec = 0.1f;
    public float requiredProgress = 1f;
    public bool stayDarkUntilSuccess = true;

    [Header("Darkness vs Progress")]
    [Range(0f, 1f)] public float darkAlphaAtZero = 0.85f;
    [Range(0f, 1f)] public float lightAlphaAtFull = 0.25f;
    public float overlayFadeSpeed = 2f;

    [Header("Camera Sway")]
    public float swayAmplitude = 2f;
    public float swayFrequency = 2f;

    [Header("Look Damp")]
    public float drowsySensitivityMultiplier = 0.35f;

    [Header("Events")]
    public UnityEvent onStartDrowsy;
    public UnityEvent onSuccess;
    public UnityEvent onFail;

    // 내부 상태
    bool active;
    bool keepDark;
    float timer;
    float progress;
    float overlayAlpha;
    Quaternion rollBase;
    float originalLookMultiplier = 1f;

    // 메시지 상태
    float msgAlpha, msgStateTime;
    enum MsgState { Hidden, FadingIn, Holding, FadingOut }
    MsgState msgState = MsgState.Hidden;

    public bool IsActive => active;

    void Awake()
    {
        // 자동 시작을 강제로 금지(인스펙터에 켜져 있어도 무시)
        autoStartOnPlay = false;

        if (cam == null) cam = Camera.main;
        if (rollTarget == null && cam != null) rollTarget = cam.transform;

        if (blackOverlay != null)
        {
            SetOverlayAlpha(0f);
            blackOverlay.gameObject.SetActive(false); // 시작 시 꺼둠
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
        }

        if (messageText != null)
        {
            var c = messageText.color;
            messageText.color = new Color(c.r, c.g, c.b, 0f);
            messageText.text = "";
        }

        if (lookRef == null) lookRef = GetComponentInParent<CarLocalFirstPersonLook>();
        if (lookRef != null) originalLookMultiplier = lookRef.sensitivityMultiplier;

        if (rollTarget != null) rollBase = rollTarget.localRotation;

        HideUIImmediate(); // 시작 전에 확실히 숨김
    }

    void OnEnable()
    {
        HideUIImmediate();
    }

    void OnDisable()
    {
        HideUIImmediate();
    }

    void Start()
    {
        // 자동 시작 금지. 필요 시 외부에서 StartMinigame() 호출
    }

    void Update()
    {
        // 진행도에 따른 오버레이
        float targetAlpha = 0f;
        if (active || keepDark)
        {
            float p = Mathf.Clamp01(requiredProgress > 0f ? (progress / requiredProgress) : 0f);
            targetAlpha = Mathf.Lerp(darkAlphaAtZero, lightAlphaAtFull, p);
        }
        overlayAlpha = Mathf.MoveTowards(overlayAlpha, targetAlpha, overlayFadeSpeed * Time.deltaTime);
        if (blackOverlay != null) SetOverlayAlpha(overlayAlpha);

        UpdateMessage();

        // 비활성 상태에서는 혹시 켜져 있어도 모두 끔(안전장치)
        if (!active)
        {
            if (uiRoot && uiRoot.activeSelf) uiRoot.SetActive(false);
            if (progressSlider && progressSlider.gameObject.activeSelf) progressSlider.gameObject.SetActive(false);
            if (messageText && messageText.gameObject.activeSelf) messageText.gameObject.SetActive(false);
            if (blackOverlay && blackOverlay.gameObject.activeSelf) blackOverlay.gameObject.SetActive(false);
            return;
        }

        // 시간 제한
        if (useTimeLimit && timeLimit > 0f)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                Fail();
                return;
            }
        }

        // 진행도 감소
        progress = Mathf.Max(0f, progress - progressDecayPerSec * Time.deltaTime);
        UpdateUI();

        // 입력 처리
        if (Input.GetMouseButtonDown(0))
        {
            bool ok = (clickMode == ClickMode.Anywhere);
            if (!ok && cam != null)
            {
                Vector3 sp = (Cursor.lockState == CursorLockMode.Locked)
                    ? new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f)
                    : Input.mousePosition;

                Ray ray = cam.ScreenPointToRay(sp);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, wheelMask))
                    ok = (steeringWheelCollider == null) ? true : (hit.collider == steeringWheelCollider);
            }

            if (ok)
            {
                progress = Mathf.Min(requiredProgress, progress + progressPerClick);
                UpdateUI();
                if (progress >= requiredProgress)
                {
                    Success();
                    return;
                }
            }
        }

        // 화면 흔들림
        if (rollTarget != null)
        {
            float roll = Mathf.Sin(Time.time * Mathf.PI * 2f * swayFrequency) * swayAmplitude;
            rollTarget.localRotation = Quaternion.AngleAxis(roll, Vector3.forward) * rollBase;
        }
    }

    public void StartMinigame()
    {
        if (active) return;

        active = true;
        keepDark = stayDarkUntilSuccess;
        timer = timeLimit;
        progress = 0f;
        overlayAlpha = 0f;
        UpdateUI();

        if (lookRef != null) lookRef.sensitivityMultiplier = drowsySensitivityMultiplier;

        ShowUI();                 // Slider/Text/BlackOverlay 켬
        ShowMessage(startMessage);

        onStartDrowsy?.Invoke();
    }

    public void ForceStop() // 필요 시 외부에서 강제 종료용
    {
        if (!active) return;
        Fail();
    }

    void Success()
    {
        active = false;
        keepDark = false;
        RestoreLookAndRoll();
        HideMessageImmediate();
        HideUIImmediate();
        onSuccess?.Invoke();
    }

    void Fail()
    {
        active = false;
        keepDark = false;
        RestoreLookAndRoll();
        HideMessageImmediate();
        HideUIImmediate();
        onFail?.Invoke();
    }

    void RestoreLookAndRoll()
    {
        if (rollTarget != null) rollTarget.localRotation = rollBase;
        if (lookRef != null) lookRef.sensitivityMultiplier = originalLookMultiplier;
    }

    void UpdateUI()
    {
        if (progressSlider != null)
            progressSlider.value = Mathf.Clamp01(requiredProgress > 0f ? (progress / requiredProgress) : 0f);
    }

    void SetOverlayAlpha(float a)
    {
        if (blackOverlay == null) return;
        var c = blackOverlay.color;
        blackOverlay.color = new Color(c.r, c.g, c.b, a);
    }

    // ---------- UI 토글 ----------
    void ShowUI()
    {
        if (uiRoot != null) uiRoot.SetActive(true);
        if (progressSlider != null) progressSlider.gameObject.SetActive(true);
        if (messageText != null) messageText.gameObject.SetActive(true);
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(true);
    }

    void HideUIImmediate()
    {
        if (uiRoot != null) uiRoot.SetActive(false);
        if (progressSlider != null) progressSlider.gameObject.SetActive(false);
        if (messageText != null) messageText.gameObject.SetActive(false);
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);

        if (messageText != null)
        {
            var c = messageText.color;
            messageText.color = new Color(c.r, c.g, c.b, 0f);
            messageText.text = "";
        }

        overlayAlpha = 0f;
        if (blackOverlay != null) SetOverlayAlpha(0f);
    }

    // ---------- 메시지 ----------
    void ShowMessage(string msg)
    {
        if (messageText == null) return;
        if (!messageText.gameObject.activeSelf) messageText.gameObject.SetActive(true);
        messageText.text = msg;
        msgState = MsgState.FadingIn;
        msgStateTime = 0f;
        msgAlpha = 0f;
        ApplyMsgAlpha();
    }

    void HideMessageImmediate()
    {
        if (messageText == null) return;
        msgState = MsgState.Hidden;
        msgAlpha = 0f;
        ApplyMsgAlpha();
        messageText.text = "";
        messageText.gameObject.SetActive(false);
    }

    void UpdateMessage()
    {
        if (messageText == null) return;
        msgStateTime += Time.deltaTime;

        switch (msgState)
        {
            case MsgState.FadingIn:
                msgAlpha = Mathf.Clamp01(msgStateTime / Mathf.Max(0.0001f, msgFadeIn));
                ApplyMsgAlpha();
                if (msgAlpha >= 1f)
                {
                    msgState = MsgState.Holding;
                    msgStateTime = 0f;
                }
                break;
            case MsgState.Holding:
                if (msgHold >= 0f && msgStateTime >= msgHold)
                {
                    msgState = MsgState.FadingOut;
                    msgStateTime = 0f;
                }
                break;
            case MsgState.FadingOut:
                msgAlpha = 1f - Mathf.Clamp01(msgStateTime / Mathf.Max(0.0001f, msgFadeOut));
                ApplyMsgAlpha();
                if (msgAlpha <= 0f)
                {
                    msgState = MsgState.Hidden;
                    messageText.text = "";
                }
                break;
        }
    }

    void ApplyMsgAlpha()
    {
        var c = messageText.color;
        messageText.color = new Color(c.r, c.g, c.b, msgAlpha);
    }
}