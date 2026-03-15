using UnityEngine;

public class GameManagerTester : MonoBehaviour
{
    public static GameManagerTester Instance { get; private set; }

    private void Awake()
    {
        // Setup Singleton chuẩn
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ GameManagerTester qua các Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // Đăng ký lắng nghe toàn bộ các sự kiện quan trọng để theo dõi luồng
        GameEvents.OnGameStateChanged += (state) => Debug.Log($"<color=cyan>[STATE]</color> Game chuyển sang: {state}");
        GameEvents.OnCharacterSelected += (pID, cID) => Debug.Log($"<color=yellow>[SELECT]</color> Player {pID} đã chọn tướng {cID}");
        GameEvents.OnAllCharactersSelected += () => Debug.Log("<color=green>[SYSTEM]</color> Mọi người đã chọn xong! Chuẩn bị đấu.");
        GameEvents.OnRoundStarted += (round) => Debug.Log($"<color=orange>[ROUND]</color> BẮT ĐẦU HIỆP {round}!");
        GameEvents.OnMatchStarted += () => Debug.Log("<color=red>[FIGHT]</color> TRẬN ĐẤU BẮT ĐẦU!!!");
        GameEvents.OnShowKO += (winner) => Debug.Log($"<color=magenta>[K.O]</color> Hiệp này Player {winner} thắng!");
        GameEvents.OnCoreSelectionStarted += (playerId) => Debug.Log($"<color=blue>[CORE]</color> Đang mở bảng chọn Lõi cho player {playerId}");
    }

    private void Update()
    {
        // 1. Nhấn phím 1 và 2 để giả lập P1 và P2 chọn tướng
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameEvents.RaiseCharacterSelected(1, 101); // Giả lập P1 chọn tướng ID 101
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameEvents.RaiseCharacterSelected(2, 202); // Giả lập P2 chọn tướng ID 202
        }

        // 2. Nhấn K để giả lập Player 1 bị hạ gục (Kết thúc hiệp)
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("--- Giả lập Player 1 chết ---");
            GameEvents.RaisePlayerDied(1); // 1 là ID của P1
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("--- Giả lập  Player 2 chết ---");
            GameEvents.RaisePlayerDied(2); // 1 là ID của P1
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("--- Giả lập chọn Bật lõi ---");
            // Gọi sự kiện để GameManager biết và chạy StartNextRound
            GameEvents.RaiseCoreSelectionStarted(1);
        }
        // 3. Nhấn C để giả lập đã chọn xong Lõi (Bắt đầu hiệp tiếp theo)
        if (Input.GetKeyDown(KeyCode.F4))
        {
            Debug.Log("--- Giả lập chọn Lõi xong ---");
            // Gọi sự kiện để GameManager biết và chạy StartNextRound
            GameEvents.RaiseCoreSelectionFinished();
        }

        // 4. Nhấn Space để ép bắt đầu ngay (Dùng cho test nhanh)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("TESTER: Ép bắt đầu trận đấu...");
            GameEvents.RaiseAllCharactersSelected();
        }
    }
}