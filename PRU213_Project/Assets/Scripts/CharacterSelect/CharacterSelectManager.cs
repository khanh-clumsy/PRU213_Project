using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CharacterSelectManager : MonoBehaviour
{
    [Header("UI Text")]
    public TMP_Text turnText;
    public TMP_Text infoText;

    [Header("Optional Confirm Button")]
    public Button startBattleButton;

    private int currentPlayer = 1;
    private bool player1Done = false;
    private bool player2Done = false;

    private void Start()
    {
        if (GameData.Instance != null)
        {
            GameData.Instance.player1Character = -1;
            GameData.Instance.player2Character = -1;
        }

        currentPlayer = 1;
        UpdateUI();

        if (startBattleButton != null)
            startBattleButton.gameObject.SetActive(false);
    }

    public void SelectCharacter(int characterID)
    {
        if (GameData.Instance == null) return;

        if (currentPlayer == 1)
        {
            GameData.Instance.player1Character = characterID;
            player1Done = true;
            currentPlayer = 2;
            UpdateUI();
        }
        else if (currentPlayer == 2)
        {
            if (characterID == GameData.Instance.player1Character)
            {
                if (infoText != null)
                    infoText.text = "Player 2 không ???c ch?n trùng Player 1!";
                return;
            }

            GameData.Instance.player2Character = characterID;
            player2Done = true;
            UpdateUI();

            if (startBattleButton != null)
                startBattleButton.gameObject.SetActive(true);
            else
                StartBattle();
        }
    }

    public void StartBattle()
    {
        if (GameData.Instance == null) return;

        if (GameData.Instance.player1Character == -1 || GameData.Instance.player2Character == -1)
        {
            if (infoText != null)
                infoText.text = "Hãy ch?n ?? 2 nhân v?t tr??c!";
            return;
        }

        SceneManager.LoadScene("BattleScene");
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("StartScene");
    }

    private void UpdateUI()
    {
        if (turnText != null)
        {
            if (!player1Done)
                turnText.text = "Player 1 ch?n nhân v?t";
            else if (!player2Done)
                turnText.text = "Player 2 ch?n nhân v?t";
            else
                turnText.text = "?ã ch?n xong!";
        }

        if (infoText != null && GameData.Instance != null)
        {
            string p1 = GetCharacterName(GameData.Instance.player1Character);
            string p2 = GetCharacterName(GameData.Instance.player2Character);

            infoText.text = "P1: " + p1 + " | P2: " + p2;
        }
    }

    private string GetCharacterName(int id)
    {
        switch (id)
        {
            case 0: return "Naruto";
            case 1: return "Sasuke";
            case 2: return "Sakura";
            case 3: return "Kakashi";
            case 4: return "Lee";
            default: return "Ch?a ch?n";
        }
    }
}