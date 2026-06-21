using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public bool hasEatenFood;

    [SerializeField]
    private GameObject foodObject;

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

    private void Update()
    {
        if (!hasEatenFood && foodObject == null)
        {
            hasEatenFood = true;
            Debug.Log("음식을 먹었다!");
        }
    }
}