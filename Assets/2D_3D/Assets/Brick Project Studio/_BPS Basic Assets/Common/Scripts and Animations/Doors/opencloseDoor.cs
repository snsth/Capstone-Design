using System.Collections;
using UnityEngine;

namespace SojaExiles
{
    public class OpenCloseDoor : MonoBehaviour
    {
        [SerializeField] private Animator openAndClose;
        [SerializeField] private float interactDistance = 3f;
        [SerializeField]
        private GameObject interactText;

        private Transform player;
        private bool open;

        private void Start()
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag("Player");

            if (interactText != null)
            {
                interactText.SetActive(false);
            }

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        private void Update()
        {
            if (player == null)
                return;

            float distance =
                Vector3.Distance(player.position, transform.position);

            bool canInteract = distance < interactDistance;

            // 문이 닫혀 있을 때만 표시
            if (interactText != null)
            {
                interactText.SetActive(canInteract && !open);
            }

            if (!canInteract)
                return;

            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!open)
                {
                    StartCoroutine(Opening());
                }
                else
                {
                    StartCoroutine(Closing());
                }
            }
        }

        IEnumerator Opening()
        {
            if (interactText != null)
            {
                interactText.SetActive(false);
            }

            openAndClose.Play("Opening");
            open = true;

            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator Closing()
        {
            openAndClose.Play("Closing");
            open = false;

            yield return new WaitForSeconds(0.5f);
        }
    }
}