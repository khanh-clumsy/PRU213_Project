using TMPro;
using UnityEngine;

public class ScoreUIHandler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private void OnEnable()
    {
        GameEvents.OnScoreChanged += UpdateScoreUI;
    }

    private void OnDisable()
    {
        GameEvents.OnScoreChanged -= UpdateScoreUI;
    }

    private void Start()
    {
        if (scoreText != null)
        {
            scoreText.text = "";
            scoreText.enabled = false;   // ẩn text, không tắt cả object
        }
    }

    private void UpdateScoreUI(int p1Score, int p2Score)
    {
        Debug.Log($"[ScoreUI] UpdateScoreUI called: {p1Score} - {p2Score}");

        if (scoreText == null) return;

        scoreText.enabled = true;
        scoreText.text = p1Score + " - " + p2Score;
    }
}