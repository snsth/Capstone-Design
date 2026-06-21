using UnityEngine;

public class BR_FlashlightController : MonoBehaviour
{
    [Tooltip("씬에 만들어 둔 Spotlight를 여기에 드래그")]
    public Light spotLight;

    void Awake()
    {
        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>();

        if (spotLight != null)
            spotLight.enabled = false;
    }

    public void SetActive(bool on)
    {
        if (spotLight != null)
            spotLight.enabled = on;
    }

    public bool IsOn => spotLight != null && spotLight.enabled;
}
