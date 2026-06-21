using System.Collections;
using UnityEngine;

// 형광등 깜빡임: 머티리얼 Emission + Light 컴포넌트 동시 제어
// 오브젝트에 Renderer와 Light 중 하나만 있어도 동작
public class BR_FluorescentFlicker : MonoBehaviour
{
    [Header("머티리얼 Emission")]
    [Tooltip("깜빡일 Renderer (없으면 자동 탐색)")]
    public Renderer targetRenderer;
    [Tooltip("Emission에 사용할 기본 색상")]
    public Color emissionColor = new Color(1f, 0.98f, 0.9f);
    [Tooltip("켜진 상태 Emission 강도 (HDRP는 수만~수십만)")]
    public float onIntensity = 3f;
    [Tooltip("꺼진 상태 Emission 강도")]
    public float offIntensity = 0f;

    [Header("Light 컴포넌트")]
    [Tooltip("같이 깜빡일 Light (없으면 자동 탐색)")]
    public Light flickerLight;
    [Tooltip("켜진 상태 Light 강도")]
    public float lightOnIntensity = 150000f;

    [Header("깜빡임 타이밍")]
    [Tooltip("정상 점등 유지 최소 시간(초)")]
    public float stableMinTime = 3f;
    [Tooltip("정상 점등 유지 최대 시간(초)")]
    public float stableMaxTime = 12f;
    [Tooltip("깜빡임 1회 최소 횟수")]
    public int flickerMinCount = 1;
    [Tooltip("깜빡임 1회 최대 횟수")]
    public int flickerMaxCount = 5;
    [Tooltip("깜빡임 한 번의 꺼짐 시간 (초)")]
    public float flickerOffDuration = 0.06f;
    [Tooltip("깜빡임 한 번의 켜짐 시간 (초)")]
    public float flickerOnDuration = 0.08f;

    Material mat;
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissiveColor");       // HDRP
    static readonly int EmissionColorLegacyID = Shader.PropertyToID("_EmissionColor"); // Built-in

    void Awake()
    {
        if (targetRenderer == null)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (flickerLight == null)
            flickerLight = GetComponentInChildren<Light>();

        if (targetRenderer != null)
        {
            // 인스턴스 머티리얼 복사 (다른 오브젝트에 영향 X)
            mat = targetRenderer.material;
            mat.EnableKeyword("_EMISSION");
            SetEmission(onIntensity);
        }

        if (flickerLight != null)
            flickerLight.intensity = lightOnIntensity;

        StartCoroutine(FlickerLoop());
    }

    void SetEmission(float intensity)
    {
        if (mat == null) return;
        Color c = emissionColor * intensity;
        // HDRP와 Built-in 둘 다 시도
        if (mat.HasProperty(EmissionColorID))
            mat.SetColor(EmissionColorID, c);
        else if (mat.HasProperty(EmissionColorLegacyID))
            mat.SetColor(EmissionColorLegacyID, c);
    }

    void SetLight(bool on)
    {
        if (flickerLight == null) return;
        flickerLight.intensity = on ? lightOnIntensity : 0f;
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            // 안정 점등 구간
            yield return new WaitForSeconds(Random.Range(stableMinTime, stableMaxTime));

            // 깜빡임 N회
            int count = Random.Range(flickerMinCount, flickerMaxCount + 1);
            for (int i = 0; i < count; i++)
            {
                // 꺼짐
                SetEmission(offIntensity);
                SetLight(false);
                yield return new WaitForSeconds(flickerOffDuration);

                // 켜짐
                SetEmission(onIntensity);
                SetLight(true);

                if (i < count - 1)
                    yield return new WaitForSeconds(flickerOnDuration);
            }
        }
    }
}
