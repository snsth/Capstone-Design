using System.Collections;
using UnityEngine;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    [SerializeField]
    private PlayerCameraMovement cameraMovement;

    [SerializeField]
    private TMP_Text subtitleText;

    private void Start()
    {
        cameraMovement.OnCameraAttached += ShowIntroSubtitle;
    }

    private void ShowIntroSubtitle()
    {
        StartCoroutine(ShowSubtitleRoutine());
    }

    IEnumerator ShowSubtitleRoutine()
    {
        subtitleText.gameObject.SetActive(true);

        subtitleText.text = "???";
        yield return new WaitForSeconds(2f);

        subtitleText.text = "뭐야 이거 고장났나?";
        yield return new WaitForSeconds(2f);

        subtitleText.text = "......";
        yield return new WaitForSeconds(2f);

        subtitleText.text = "그만할래";
        yield return new WaitForSeconds(1.5f);

        subtitleText.text = "밥 먹고 수영장이나 가야지";
        yield return new WaitForSeconds(2f);

        subtitleText.gameObject.SetActive(false);
    }
}