using UnityEngine;

public class JumpScareImage : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 10f;

    [SerializeField]
    private Transform targetCamera;

    private bool isMoving;

    public void Play()
    {
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving)
            return;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetCamera.position,
                moveSpeed * Time.deltaTime
            );
    }
}