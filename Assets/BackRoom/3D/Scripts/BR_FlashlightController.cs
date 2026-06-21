using UnityEngine;

// Attach to a child of the Camera. The spot light points in local +Z (camera forward).
public class BR_FlashlightController : MonoBehaviour
{
    [Header("Light Settings")]
    [Range(1f, 80f)]    public float range           = 20f;
    [Range(10f, 120f)]  public float spotAngle       = 50f;
    [Range(5f,  90f)]   public float innerSpotAngle  = 20f;
    // HDRP는 candela/lumen 단위 사용 — Built-in RP라면 5~20으로 낮출 것
    public float intensity = 150000f;
    public Color lightColor = new Color(1f, 0.97f, 0.92f);

    Light spotLight;

    void Awake()
    {
        EnsureLight();
        ApplySettings();
        spotLight.enabled = false;
    }

    // Keep Inspector sliders live during Play Mode
    void Update()
    {
        if (spotLight != null && spotLight.enabled)
            ApplySettings();
    }

    void EnsureLight()
    {
        if (spotLight != null) return;
        spotLight = GetComponent<Light>();
        if (spotLight == null) spotLight = gameObject.AddComponent<Light>();
    }

    void ApplySettings()
    {
        spotLight.type           = LightType.Spot;
        spotLight.range          = range;
        spotLight.spotAngle      = spotAngle;
        spotLight.innerSpotAngle = innerSpotAngle;
        spotLight.intensity      = intensity;
        spotLight.color   = lightColor;
        spotLight.shadows = LightShadows.Soft;
    }

    public void SetActive(bool on)
    {
        EnsureLight();
        spotLight.enabled = on;
    }

    public bool IsOn => spotLight != null && spotLight.enabled;
}
