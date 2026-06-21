using System.Collections;
using UnityEngine;
using TMPro;

public class WrongWayTrigger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField]
    private TMP_Text wrongWayText;

    [Header("Monster")]
    [SerializeField]
    private MonsterSpawner monsterSpawner;

    [SerializeField]
    private AudioSource monsterBGM;
    private bool monsterSpawned;

    [Header("Settings")]
    [SerializeField]
    private bool showOnlyOnce = true;

    [SerializeField]
    private float messageInterval = 5f;

    [TextArea]
    [SerializeField]
    private string[] warningMessages;

    private bool triggered;
    private Coroutine warningCoroutine;

    private void Start()
    {
        wrongWayText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (showOnlyOnce)
        {
            if (triggered)
                return;

            StartCoroutine(ShowWarningOnce());
        }
        else
        {
            warningCoroutine = StartCoroutine(ShowProgressiveWarnings());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!showOnlyOnce)
        {
            if (warningCoroutine != null)
            {
                StopCoroutine(warningCoroutine);
            }

            wrongWayText.gameObject.SetActive(false);
        }
    }

    private IEnumerator ShowWarningOnce()
    {
        triggered = true;

        wrongWayText.gameObject.SetActive(true);

        if (warningMessages.Length > 0)
        {
            wrongWayText.text = warningMessages[0];
        }

        yield return new WaitForSeconds(2f);

        wrongWayText.gameObject.SetActive(false);
    }

    private IEnumerator ShowProgressiveWarnings()
    {
        wrongWayText.gameObject.SetActive(true);

        int index = 0;

        while (true)
        {
            wrongWayText.text = warningMessages[index];

            if (index == warningMessages.Length - 1 && !monsterSpawned)
            {
                monsterSpawned = true;

                if (monsterSpawner != null)
                {
                    monsterSpawner.SpawnMonster();
                }

                if (monsterBGM != null)
                {
                    monsterBGM.volume = 0f;
                    monsterBGM.Play();

                    while (monsterBGM.volume < 1f)
                    {
                        monsterBGM.volume += Time.deltaTime * 0.1f;
                        yield return null;
                    }
                }
                
            }

            float t = (float)index / (warningMessages.Length - 1);

            wrongWayText.color =
                Color.Lerp(Color.white, Color.red, t);

            yield return new WaitForSeconds(messageInterval);

            if (index < warningMessages.Length - 1)
            {
                index++;
            }

            if (index == warningMessages.Length - 1)
            {
                monsterSpawner.SpawnMonster();
            }
        }
    }
}