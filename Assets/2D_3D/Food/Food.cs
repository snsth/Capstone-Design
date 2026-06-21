using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject interactText;

    [Header("Sound")]
    [SerializeField]
    private AudioClip eatSound;

    [Header("Dialogue")]
    [SerializeField]
    private SubtitleManager subtitleManager;

    [SerializeField]
    private PlayerMovement playerMovement;

    [SerializeField]
    private string[] eatDialogues;

    private bool playerInRange;

    private void Start()
    {
        if (interactText != null)
            interactText.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Eat();
        }
    }

    private void Eat()
    {
        // 먹는 소리 재생
        AudioSource.PlayClipAtPoint(
            eatSound,
            transform.position
        );

        // 대사 출력
        if (subtitleManager != null)
        {
            subtitleManager.ShowSubtitle(eatDialogues);
        }

        if (playerMovement != null)
        {
            playerMovement.EnableStamina();
        }

        // 음식 제거
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            if (interactText != null)
                interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;

            if (interactText != null)
                interactText.SetActive(false);
        }
    }
}