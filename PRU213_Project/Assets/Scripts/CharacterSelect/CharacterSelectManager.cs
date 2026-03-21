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

    public CharacterTagUI tagUI;
    private int p1Selected = -1;
    private int p2Selected = -1;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.player1CharacterID = -1;
            GameManager.Instance.player2CharacterID = -1;

            p1Selected = -1;
            p2Selected = -1;
            if (tagUI != null) tagUI.ClearAll();
        }

        currentPlayer = 1;
        UpdateUI();

        if (startBattleButton != null)
            startBattleButton.gameObject.SetActive(false);
    }

    public void SelectCharacter(int characterID)
    {
        if (GameManager.Instance == null) return;

        if (currentPlayer == 1)
        {
            // XÓA P1 cũ (nếu có)
            if (tagUI != null) tagUI.ClearTag("P1");

            // Chọn P1
            GameEvents.RaiseCharacterSelected(1, characterID);
            p1Selected = characterID;

            // HIỆN P1 trên nhân vật vừa chọn
            if (tagUI != null) tagUI.SetTag(characterID, "P1");

            player1Done = true;
            currentPlayer = 2;
            UpdateUI();
        }
        else if (currentPlayer == 2)
        {
            if (characterID == GameManager.Instance.player1CharacterID)
            {
                if (infoText != null)
                    infoText.text = "Player 2 cannot pick the same character as Player 1!";
                return;
            }

            // XÓA P2 cũ (nếu có)
            if (tagUI != null) tagUI.ClearTag("P2");

            // Chọn P2
            GameEvents.RaiseCharacterSelected(2, characterID);
            p2Selected = characterID;

            // HIỆN P2 trên nhân vật vừa chọn
            if (tagUI != null) tagUI.SetTag(characterID, "P2");

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
        if (GameManager.Instance == null) return;

        // Kiểm tra an toàn: Cả 2 phải chọn xong mới cho phép bắt đầu
        if (GameManager.Instance.player1CharacterID != -1 && GameManager.Instance.player2CharacterID != -1)
        {
            // Phát sự kiện báo cho GameManager biết mọi thứ đã sẵn sàng
            // GameManager sẽ quản lý việc tải cảnh, spawn người chơi, và bắt đầu trận đấu
            GameEvents.RaiseAllCharactersSelected();
        }
        else
        {
            if (infoText != null)
                infoText.text = "Please select both characters first!";
        }
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
                turnText.text = "Player 1, select your character";
            else if (!player2Done)
                turnText.text = "Player 2, select your character";
            else
                turnText.text = "All characters selected!";
        }

        if (infoText != null && GameManager.Instance != null)
        {
            string p1 = GetCharacterName(GameManager.Instance.player1CharacterID);
            string p2 = GetCharacterName(GameManager.Instance.player2CharacterID);

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
            default: return "None";
        }
    }
}