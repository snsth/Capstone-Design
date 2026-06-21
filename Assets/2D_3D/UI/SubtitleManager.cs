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

        string[] introLines =
        {
            "???",
            "뭐야 이거 고장났나?",
            "......",
            "그만할래",
            "밥 먹고 수영장이나 가야지"
        };

        ShowSubtitle(introLines);
    }

    /// <summary>
    /// 외부에서 호출하는 함수
    /// </summary>
    public void ShowSubtitle(string[] lines)
    {
        StartCoroutine(ShowSubtitleRoutine(lines));
    }

    private IEnumerator ShowSubtitleRoutine(string[] lines)
    {
        subtitleText.gameObject.SetActive(true);

        foreach (string line in lines)
        {
            subtitleText.text = line;
            yield return new WaitForSeconds(2f);
        }

        subtitleText.gameObject.SetActive(false);

    }
}