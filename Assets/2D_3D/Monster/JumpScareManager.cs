using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpScareManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Camera jumpScareCamera;

    [Header("Audio")]
    [SerializeField]
    private AudioSource jumpScareSound;

    [SerializeField]
    private JumpScareImage jumpScareImage;

    private bool isPlaying;

    public void StartJumpScare()
    {
        if (isPlaying)
            return;

        StartCoroutine(JumpScareRoutine());
    }

    IEnumerator JumpScareRoutine()
    {
        isPlaying = true;

        // 플레이어 입력 정지
        PlayerMovement player =
            FindFirstObjectByType<PlayerMovement>();

        if (player != null)
        {
            player.enabled = false;
        }

        // 카메라 전환
        mainCamera.enabled = false;
        jumpScareCamera.enabled = true;
        jumpScareImage.Play();
        // 사운드
        if (jumpScareSound != null)
        {
            jumpScareSound.Play();
        }


        yield return new WaitForSeconds(3f);

        SceneManager.LoadScene(0);
    }
}