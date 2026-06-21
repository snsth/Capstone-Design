using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BR_Door : MonoBehaviour
{
    public string requiredItemId = "key_01";
    public string requiredItemName = "열쇠";
    public bool consumeItem = false;

    [Header("탈출 설정")]
    public bool isExitDoor = false;
    public string nextSceneName = "";

    [Header("슬라이드 설정")]
    [Tooltip("실제로 움직일 오브젝트. 비워두면 이 오브젝트 자신을 움직임")]
    public Transform targetTransform;
    [Tooltip("벽이 빠질 방향. Up=위, Down=아래, Left=왼쪽, Right=오른쪽")]
    public SlideDirection slideDirection = SlideDirection.Up;
    [Tooltip("슬라이드 거리 (0이면 오브젝트 크기 자동 계산)")]
    public float slideDistance = 0f;
    public float openSpeed = 2f;

    public enum SlideDirection { Up, Down, Left, Right, Custom }
    [Tooltip("SlideDirection이 Custom일 때 사용할 방향 (월드 기준)")]
    public Vector3 customSlideAxis = Vector3.up;

    public bool IsOpen { get; private set; }
    public string RequiredItemId => requiredItemId;
    public string RequiredItemName => requiredItemName;

    Vector3 closedPos;
    Vector3 openPos;
    bool animating;

    Transform Target => targetTransform != null ? targetTransform : transform;

    void Start()
    {
        closedPos = Target.position;

        Vector3 axis = GetAxis();
        float dist = slideDistance > 0f ? slideDistance : GetAutoDistance(axis);
        openPos = closedPos + axis * dist;
    }

    Vector3 GetAxis()
    {
        return slideDirection switch
        {
            SlideDirection.Up    => Vector3.up,
            SlideDirection.Down  => Vector3.down,
            SlideDirection.Left  => -Target.right,
            SlideDirection.Right => Target.right,
            _                    => customSlideAxis.normalized,
        };
    }

    float GetAutoDistance(Vector3 axis)
    {
        var renderer = Target.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Vector3 size = renderer.bounds.size;
            float dot = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) * size.y
                      + Mathf.Abs(Vector3.Dot(axis, Vector3.right)) * size.x
                      + Mathf.Abs(Vector3.Dot(axis, Vector3.forward)) * size.z;
            return dot;
        }
        return 3f;
    }

    void Update()
    {
        if (!animating) return;
        Target.position = Vector3.Lerp(Target.position, openPos, openSpeed * Time.deltaTime);
        if (Vector3.Distance(Target.position, openPos) < 0.01f)
        {
            Target.position = openPos;
            animating = false;
        }
    }

    public void TryOpen()
    {
        if (IsOpen) return;
        if (BR_Inventory.Instance == null || !BR_Inventory.Instance.Has(requiredItemId)) return;
        if (consumeItem) BR_Inventory.Instance.Remove(requiredItemId);
        IsOpen = true;
        animating = true;

        if (isExitDoor)
            StartCoroutine(Escape());
    }

    IEnumerator Escape()
    {
        yield return new WaitForSeconds(1.5f);
        string scene = string.IsNullOrEmpty(nextSceneName)
            ? SceneManager.GetActiveScene().name
            : nextSceneName;
        SceneManager.LoadScene(scene);
    }
}
