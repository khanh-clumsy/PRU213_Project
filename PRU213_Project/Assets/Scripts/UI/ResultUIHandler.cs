using TMPro;
using UnityEngine;
using System.Collections;

public class ResultUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI koText;
    [SerializeField] private TextMeshProUGUI winnerText;

    private Coroutine hideKORoutine;
    private bool winnerShown = false;

    private void OnEnable()
    {
        GameEvents.OnShowKO += ShowKO;
        GameEvents.OnShowWinScreen += ShowWinner;
        GameEvents.OnRoundStarted += HideResultUI;
    }

    private void OnDisable()
    {
        GameEvents.OnShowKO -= ShowKO;
        GameEvents.OnShowWinScreen -= ShowWinner;
        GameEvents.OnRoundStarted -= HideResultUI;
    }

    private void Start()
    {
        if (koText != null)
            koText.gameObject.SetActive(false);

        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
    }

    private void ShowKO(int winnerID)
    {
        winnerShown = false;

        if (koText != null)
            koText.gameObject.SetActive(true);

        if (hideKORoutine != null)
            StopCoroutine(hideKORoutine);

        hideKORoutine = StartCoroutine(HideKOAfterDelay());
    }

    private IEnumerator HideKOAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        if (!winnerShown && koText != null)
            koText.gameObject.SetActive(false);
    }

    private void ShowWinner(int winnerID)
    {
        winnerShown = true;

        if (hideKORoutine != null)
        {
            StopCoroutine(hideKORoutine);
            hideKORoutine = null;
        }

        if (winnerText == null) return;

        winnerText.gameObject.SetActive(true);

        if (winnerID == 1)
            winnerText.text = "P1 WINNER";
        else if (winnerID == 2)
            winnerText.text = "P2 WINNER";
        else
            winnerText.text = "DRAW";
    }

    private void HideResultUI(int round)
    {
        winnerShown = false;

        if (hideKORoutine != null)
        {
            StopCoroutine(hideKORoutine);
            hideKORoutine = null;
        }

        if (koText != null)
            koText.gameObject.SetActive(false);

        if (winnerText != null)
            winnerText.gameObject.SetActive(false);
    }
}