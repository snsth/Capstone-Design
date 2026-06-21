using UnityEngine;

public class OptionMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionPanel;

    public void OpenOption()
    {
        optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        optionPanel.SetActive(false);
    }
}