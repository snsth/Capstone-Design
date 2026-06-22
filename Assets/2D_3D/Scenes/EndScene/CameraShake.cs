using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 startPos;
    private bool isShaking;

    [SerializeField] private float amplitude = 0.05f;
    [SerializeField] private float speed = 20f;

    private void Start()
    {
        startPos = transform.localPosition;
    }

    public void StartShake()
    {
        isShaking = true;
    }

    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = startPos;
    }

    private void Update()
    {
        if (!isShaking) return;

        float x = (Mathf.PerlinNoise(Time.time * speed, 0f) - 0.5f) * amplitude;
        float y = (Mathf.PerlinNoise(0f, Time.time * speed) - 0.5f) * amplitude;

        transform.localPosition = startPos + new Vector3(x, y, 0f);
    }
}