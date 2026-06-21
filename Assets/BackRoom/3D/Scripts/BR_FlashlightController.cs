using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BR_FlashlightController : MonoBehaviour
{
    [Header("스포트라이트 설정")]
    public float intensity  = 150000f;
    public float range      = 40f;
    public float spotAngle  = 50f;
    public float innerAngle = 22f;
    public Color lightColor = Color.white;

    Light                 spotlight;
    HDAdditionalLightData hdLight;
    bool                  initialized;

    void EnsureCreated()
    {
        if (initialized) return;
        initialized = true;

        var pc = FindAnyObjectByType<BR_PlayerController>();
        Transform parent = pc?.cameraTransform
                        ?? Camera.main?.transform
                        ?? FindAnyObjectByType<Camera>()?.transform;

        if (parent == null)
        {
            Debug.LogWarning("[BR_FlashlightController] 카메라를 찾을 수 없습니다.");
            return;
        }

        var go = new GameObject("[Flashlight]");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0.08f, -0.04f, 0.15f);
        go.transform.localRotation = Quaternion.identity;

        spotlight                = go.AddComponent<Light>();
        spotlight.type           = LightType.Spot;
        spotlight.color          = lightColor;
        spotlight.range          = range;
        spotlight.spotAngle      = spotAngle;
        spotlight.innerSpotAngle = innerAngle;
        spotlight.shadows        = LightShadows.Soft;
        spotlight.enabled        = false;

        hdLight           = go.AddComponent<HDAdditionalLightData>();
        hdLight.intensity = intensity;
        hdLight.SetSpotAngle(spotAngle, innerAngle);
        hdLight.affectDiffuse  = true;
        hdLight.affectSpecular = true;
    }

    public void SetActive(bool on)
    {
        EnsureCreated();
        if (spotlight == null) return;
        spotlight.enabled = on;

        // 손전등 ON 시 자동노출 보정 — 다른 빛에 묻히지 않도록
        BR_PostProcessing.Instance?.SetFlashlightExposureBias(on ? -1.5f : 0f);
    }

    public bool IsOn => spotlight != null && spotlight.enabled;
}
