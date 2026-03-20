using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float numberDisplayTime = 1f;
    [SerializeField] private float fightDisplayTime = 1f;

    private Coroutine currentRoutine;

    private void OnEnable()
    {
        GameEvents.OnCountdownTick += ShowCountdown;
        GameEvents.OnMatchStarted += ShowFight;
    }

    private void OnDisable()
    {
        GameEvents.OnCountdownTick -= ShowCountdown;
        GameEvents.OnMatchStarted -= ShowFight;
    }

    private void Start()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
    }

    private void ShowCountdown(int count)
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowTextRoutine(count.ToString(), numberDisplayTime));
    }

    private void ShowFight()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(ShowTextRoutine("FIGHT!", fightDisplayTime));
    }

    private IEnumerator ShowTextRoutine(string message, float duration)
    {
        countdownText.text = message;
        countdownText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        countdownText.gameObject.SetActive(false);
    }
}