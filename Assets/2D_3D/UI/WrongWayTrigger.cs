using System.Collections;
using UnityEngine;
using TMPro;

public class WrongWayTrigger : MonoBehaviour
{
    public TMP_Text wrongWayText;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        StartCoroutine(ShowWarning());
    }

    IEnumerator ShowWarning()
    {
        triggered = true;

        wrongWayText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        wrongWayText.gameObject.SetActive(false);

        
        triggered = true; // 한 번만 나오게 할 거면 유지
    }
}