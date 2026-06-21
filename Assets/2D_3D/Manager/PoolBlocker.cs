using System.Collections;
using UnityEngine;
using TMPro;

public class PoolBlocker : MonoBehaviour
{
    [SerializeField]
    private TMP_Text messageText;

    private bool showingMessage;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // 이미 음식 먹었으면 통과
        if (GameState.Instance.hasEatenFood)
            return;

        if (!showingMessage)
        {
            StartCoroutine(ShowMessage());
        }

        // 플레이어를 뒤로 밀기
        CharacterController controller =
            other.GetComponent<CharacterController>();

        if (controller != null)
        {
            Vector3 pushDirection =
                -other.transform.forward;

            controller.Move(pushDirection * 2f);
        }
    }

    IEnumerator ShowMessage()
    {
        showingMessage = true;

        messageText.gameObject.SetActive(true);
        messageText.text = "밥을 먹어야 뛸 수 있을 것 같다.";

        yield return new WaitForSeconds(2f);

        messageText.gameObject.SetActive(false);

        showingMessage = false;
    }
}