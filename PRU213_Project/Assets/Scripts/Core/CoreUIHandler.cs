using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CoreUIHandler : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject corePanel;
    public Transform cardContainer;
    public GameObject cardPrefab;

    [Header("Data")]
    public List<CoreData> allCores;       // Danh sách tất cả các lõi bạn đã tạo (ScriptableObjects)

    [Header("UI Elements")]
    public TextMeshProUGUI turnIndicatorText; // Text hiển thị "Lượt của Player 1/2"
    private int currentPlayerSelecting = 0;

    private List<GameObject> activeCards = new List<GameObject>();

    private void OnEnable()
    {
        // Đăng ký: Khi nghe thấy sự kiện CoreSelectionStarted thì gọi hàm ShowRandomCores
        GameEvents.OnCoreSelectionStarted += OpenSelectionForPlayer;
    }

    private void OnDisable()
    {
        // Hủy đăng ký để tránh lỗi bộ nhớ
        GameEvents.OnCoreSelectionStarted -= OpenSelectionForPlayer;
    }

    private void OpenSelectionForPlayer(int playerID)
    {
        currentPlayerSelecting = playerID;
        corePanel.SetActive(true);
        Time.timeScale = 0; // Dừng game

        // Cập nhật UI thông báo lượt
        if (turnIndicatorText != null)
            turnIndicatorText.text = $"PLAYER {playerID} CHOOSE A CORE";

        ShowRandomCores(); // Hàm tạo 3 thẻ bài của bạn
    }

    // Hàm này gọi để bắt đầu quá trình chọn lõi (ví dụ gọi từ GameManager)
    public void ShowRandomCores()
    {
        corePanel.SetActive(true);
        Time.timeScale = 0; // Tạm dừng game để người chơi chọn

        // Xóa các thẻ cũ nếu có
        ClearOldCards();

        // Xáo trộn danh sách và lấy ra 3 lõi ngẫu nhiên
        List<CoreData> randomSelection = GetRandomCores(3);

        // Tạo ra 3 thẻ bài từ Prefab
        foreach (CoreData data in randomSelection)
        {
            GameObject cardObj = Instantiate(cardPrefab, cardContainer);
            Core cardScript = cardObj.GetComponent<Core>();

            if (cardScript != null)
            {
                cardScript.Setup(data, this);
            }
            activeCards.Add(cardObj);
        }
    }

    private bool isSelecting = false; // Biến kiểm soát trạng thái đang chọn

    public void OnCoreSelected(Core selectedCard, CoreData selectedData)
    {
        if (isSelecting) return;
        isSelecting = true;

        Debug.Log($"<color=yellow>Player {currentPlayerSelecting} đang chạy hiệu ứng chọn lõi: {selectedData.coreName}</color>");

        StartCoroutine(HandleSelectionSequence(selectedCard, selectedData));
    }

    private IEnumerator HandleSelectionSequence(Core selectedCard, CoreData selectedData)
    {
        yield return StartCoroutine(SelectRoutine(selectedCard, selectedData, currentPlayerSelecting));

        if (currentPlayerSelecting == 1)
        {
            isSelecting = false;
            Debug.Log("P1 đã chọn xong, làm mới bảng cho P2");
            OpenSelectionForPlayer(2);
        }
        else if (currentPlayerSelecting == 2)
        {
            isSelecting = false;
            currentPlayerSelecting = 0;

            corePanel.SetActive(false);
            Time.timeScale = 1;

            Debug.Log("Cả 2 đã chọn xong, báo cáo cho GameManager!");
            if (turnIndicatorText != null)
                turnIndicatorText.text = "";
            GameEvents.RaiseCoreSelectionFinished();
        }
    }
    private IEnumerator SelectRoutine(Core selectedCard, CoreData data, int playerID)
    {
        // BƯỚC 2: Vô hiệu hóa tương tác của TẤT CẢ các thẻ ngay lập tức
        foreach (GameObject cardObj in activeCards)
        {
            // Tắt component Button để không thể click
            Button btn = cardObj.GetComponentInChildren<Button>();
            if (btn != null) btn.interactable = false;
        }

        // 1. Chạy hiệu ứng cho 3 thẻ (giữ nguyên logic của bạn)
        foreach (GameObject card in activeCards)
        {
            Core cardScript = card.GetComponent<Core>();
            if (card == selectedCard.gameObject)
            {
                // Thẻ được chọn: Phóng to
                card.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                cardScript.cardBackground.color = new Color(1f, 0.9f, 0.2f, 1f);

                // Đổi màu Outline sang Trắng để tạo hiệu ứng phát sáng
                cardScript.cardOutline.effectColor = Color.white;
                cardScript.cardOutline.effectDistance = new Vector2(5, -5);
            }
            else
            {
                // Thẻ không được chọn: Thu nhỏ
                card.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
                cardScript.cardBackground.color = new Color(0.3f, 0.3f, 0.3f, 1f);
                cardScript.cardOutline.effectColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            }
        }

        // 2. DỪNG HÌNH 0.6 GIÂY
        yield return new WaitForSecondsRealtime(0.6f);

        // 3. Áp dụng chỉ số cho nhân vật
        ApplyCoreEffect(data, playerID);

        // 4. Đóng UI, khôi phục thời gian và dọn rác
        corePanel.SetActive(false);
        Time.timeScale = 1;
        ClearOldCards();

        // BƯỚC 3: Reset biến cờ hiệu để lần mở bảng tiếp theo có thể chọn được
        isSelecting = false;
    }

    //private CoreData GenerateRandomCore()
    //{
    //    // 1. Tạo một instance mới của CoreData trong bộ nhớ (không tạo file trên đĩa)
    //    CoreData randomCore = ScriptableObject.CreateInstance<CoreData>();

    //    // 2. Ngẫu nhiên chọn loại thuộc tính (HP, Speed, hoặc Defense)
    //    int typeIndex = Random.Range(0, 3);
    //    randomCore.type = (CoreData.CoreType)typeIndex;

    //    // 3. Ngẫu nhiên giá trị (Có thể âm hoặc dương)
    //    // Ví dụ: từ -20 đến +50
    //    float randomValue = Random.Range(-20f, 51f);
    //    randomCore.value = randomValue;

    //    // 4. Thiết lập Tên và Mô tả dựa trên giá trị ngẫu nhiên
    //    string prefix = randomValue >= 0 ? "Cường Hóa" : "Nguyền Rủa";
    //    string traitName = "";

    //    switch (randomCore.type)
    //    {
    //        case CoreData.CoreType.MaxHP:
    //            traitName = "Sinh Lực";
    //            randomCore.description = (randomValue >= 0 ? "Tăng " : "Giảm ") + Mathf.Abs(randomValue) + " Máu tối đa";
    //            break;
    //        case CoreData.CoreType.MoveSpeed:
    //            traitName = "Tốc Độ";
    //            randomCore.description = (randomValue >= 0 ? "Tăng " : "Giảm ") + Mathf.Abs(randomValue) + " Tốc độ di chuyển";
    //            break;
    //        case CoreData.CoreType.Defense:
    //            traitName = "Phòng Thủ";
    //            randomCore.description = (randomValue >= 0 ? "Tăng " : "Giảm ") + Mathf.Abs(randomValue) + " Giáp";
    //            break;
    //    }

    //    randomCore.coreName = prefix + " " + traitName;

    //    // 5. Gán Icon mặc định (Bạn nên có 1 list icon sẵn để bốc ngẫu nhiên)
    //    // randomCore.icon = defaultIcon; 

    //    return randomCore;
    //}

    private void ApplyCoreEffect(CoreData core, int playerID)
    {
        Player player = GameManager.Instance.GetPlayer(playerID);
        if (player == null)
        {
            Debug.LogError($"Không tìm thấy Player với ID {playerID}");
            return;
        }
        foreach (StatModifier modifier in core.modifiers)
        {
            switch (modifier.type)
            {
                case CoreType.MaxHP:
                    // Tăng Max HP và hồi máu tương ứng
                    player.ModifyMaxHP((int)modifier.value);

                    // Cập nhật UI ngay lập tức thông qua Event
                    //GameEvents.RaiseHealthChanged(playerID, player.currentHP);
                    Debug.Log($"P{playerID} nhận {core.coreName}: Max HP +{modifier.value}");
                    break;

                case CoreType.MoveSpeed:
                    PlayerMovement movement = player.GetComponent<PlayerMovement>();
                    if (movement != null)
                    {
                        // 2. Thay đổi giá trị moveSpeed bên trong script đó
                        movement.moveSpeed += modifier.value;
                        Debug.Log($"P{playerID} nhận {core.coreName}: Tốc chạy mới là {movement.moveSpeed}");
                    }
                    else
                    {
                        Debug.LogWarning($"Không tìm thấy script PlayerMovement trên Player {playerID}!");
                    }
                    break;

                case CoreType.Attack:
                    // Giả sử script Player có biến attackDamage
                    player.ModifyAttackDamage((int)(modifier.value));
                    Debug.Log($"P{playerID} nhận {core.coreName}: Sát thương +{modifier.value}");
                    break;
                case CoreType.Mana:
                    player.ModifyCurrentMana((int)(modifier.value));
                    Debug.Log($"P{playerID} nhận {core.coreName}: Mana +{modifier.value}");
                    break;
            }
        }
    }

    private List<CoreData> GetRandomCores(int count)
    {
        List<CoreData> pool = new List<CoreData>(allCores);
        List<CoreData> result = new List<CoreData>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index); // Không bốc trùng
        }
        return result;
    }

    private void ClearOldCards()
    {
        foreach (GameObject card in activeCards)
        {
            Destroy(card);
        }
        activeCards.Clear();
    }
}