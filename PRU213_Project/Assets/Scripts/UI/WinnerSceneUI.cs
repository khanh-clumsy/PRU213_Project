using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinnerSceneUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI winnerText;

    private void Start()
    {
        int winnerID = PlayerPrefs.GetInt("WinnerID", 0);

        if (winnerText == null) return;

        if (winnerID == 1)
            winnerText.text = "P1 WINNER";
        else if (winnerID == 2)
            winnerText.text = "P2 WINNER";
        else
            winnerText.text = "DRAW";
    }

    public void BackToStartScene()
    {
        SceneManager.LoadScene("StartScene");
    }
}