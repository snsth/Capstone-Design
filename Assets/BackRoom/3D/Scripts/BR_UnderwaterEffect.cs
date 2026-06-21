using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class BR_UnderwaterEffect : MonoBehaviour
{
    [Header("References")]
    [Tooltip("플레이어 컨트롤러 (없으면 자동 탐색)")]
    public BR_PlayerController player;
    [Tooltip("언더워터 효과용 Volume (없으면 자동 생성)")]
    public Volume underwaterVolume;

    [Header("언더워터 포그 색상")]
    public Color fogColor = new Color(0.05f, 0.15f, 0.3f, 1f);
    [Tooltip("포그 시작 거리 (카메라 기준)")]
    public float fogStart = 0f;
    [Tooltip("포그 끝 거리 — 짧을수록 더 어둡고 좁아 보임")]
    public float fogEnd = 20f;
    [Tooltip("효과 전환 속도")]
    public float transitionSpeed = 4f;

    Fog hdFog;
    float targetWeight;

    void Start()
    {
        if (player == null)
            player = FindObjectOfType<BR_PlayerController>();

        if (underwaterVolume == null)
            underwaterVolume = CreateVolume();

        underwaterVolume.profile.TryGet(out hdFog);
        underwaterVolume.weight = 0f;
    }

    void Update()
    {
        if (player == null) return;

        bool submerged = player.IsSubmerged;
        targetWeight = submerged ? 1f : 0f;
        underwaterVolume.weight = Mathf.Lerp(underwaterVolume.weight, targetWeight, transitionSpeed * Time.deltaTime);
    }

    Volume CreateVolume()
    {
        var go = new GameObject("UnderwaterVolume");
        var vol = go.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 10;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();

        var fog = profile.Add<Fog>(true);
        fog.enabled.Override(true);
        fog.color.Override(fogColor);
        fog.enableVolumetricFog.Override(false);
        fog.meanFreePath.Override(fogEnd);
        fog.baseHeight.Override(-9999f);
        fog.maximumHeight.Override(9999f);

        vol.profile = profile;
        return vol;
    }
}
