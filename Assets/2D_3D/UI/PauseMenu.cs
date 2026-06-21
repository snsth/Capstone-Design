using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionPanel;

    [Header("Player")]
    [SerializeField] private MonoBehaviour playerMovement;
    [SerializeField]
    private PlayerCameraMovement playerCameraMovement;

    private bool isPaused;

    private void Start()
    {
        pausePanel.SetActive(false);
        optionPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // ESC는 열기만 가능
        if (!isPaused && Input.GetKeyDown(KeyCode.Escape))
        {
            OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        isPaused = true;

        pausePanel.SetActive(true);
        optionPanel.SetActive(false);

        Time.timeScale = 0f;

        playerMovement.enabled = false;
        playerCameraMovement.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);
        optionPanel.SetActive(false);

        Time.timeScale = 1f;

        playerMovement.enabled = true;
        playerCameraMovement.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenOptionMenu()
    {
        pausePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        optionPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}