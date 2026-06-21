using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BR_Door : MonoBehaviour
{
    public string requiredItemId = "key_01";
    public string requiredItemName = "열쇠";
    public bool consumeItem = false;

    [Header("탈출 설정")]
    [Tooltip("이 문을 열면 게임 클리어 (탈출 문)")]
    public bool isExitDoor = false;
    [Tooltip("탈출 후 로드할 씬 이름 (비워두면 현재 씬 재시작)")]
    public string nextSceneName = "";

    [Header("Open Animation")]
    public Vector3 openRotationOffset = new Vector3(0f, 90f, 0f);
    public float openSpeed = 2f;

    public bool IsOpen { get; private set; }
    public string RequiredItemId => requiredItemId;
    public string RequiredItemName => requiredItemName;

    Quaternion closedRot;
    Quaternion openRot;
    bool animating;

    void Start()
    {
        closedRot = transform.localRotation;
        openRot = closedRot * Quaternion.Euler(openRotationOffset);
    }

    void Update()
    {
        if (!animating) return;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, openRot, openSpeed * Time.deltaTime);
        if (Quaternion.Angle(transform.localRotation, openRot) < 0.5f)
        {
            transform.localRotation = openRot;
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
