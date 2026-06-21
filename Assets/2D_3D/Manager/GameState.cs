using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public bool hasEatenFood;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}