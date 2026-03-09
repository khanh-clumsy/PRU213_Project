using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Định nghĩa GameState (Có thể vứt ra 1 file riêng cho sạch)
public enum GameState
{
    CharacterSelection,
    Loading,
    RoundStarting, // Trạng thái đếm ngược mỗi hiệp
    Fighting,
    RoundOver,     // Hết 1 hiệp
    CoreSelection, // Chọn lõi giữa các hiệp
    Paused,
    MatchOver      // Kết thúc cả trận đấu (ví dụ: ai thắng 2 hiệp trước)
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    public int matchDuration = 90;
    public int roundsToWin = 2; // Thắng 2 hiệp là thắng cả trận

    [Header("Current Progress")]
    public int currentRound = 1;
    public int p1RoundWins = 0;
    public int p2RoundWins = 0;


    [Header("State & Time")]
    public GameState currentState; // Chữ thường, bỏ { get; private set; }
    private float currentTime;


    [Header("Character Selection")]
    public int player1CharacterID = -1; // -1 nghĩa là chưa chọn
    public int player2CharacterID = -1;

    // Lưu máu của 2 người chơi để check xem ai thắng nếu hết giờ
    private int player1HP;
    private int player2HP;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    
    private void Awake()
    {
        // Setup Singleton chuẩn
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ GameManager qua các Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Hàm này sẽ được gọi từ Player.cs trong Awake() để đăng ký Player vào GameManager
    public void RegisterPlayer(int id, Player playerScript)
    {
        if (!players.ContainsKey(id))
        {
            players.Add(id, playerScript);
            Debug.Log($"Player {id} đã đăng ký thành công!");
        }
    }

    //Hàm lấy Player từ GameManager bằng ID (1 hoặc 2) để tiện cho các hệ thống khác gọi ra dùng
    public Player GetPlayer(int id)
    {
        if (players.TryGetValue(id, out Player player))
        {
            return player;
        }
        Debug.LogError($"Không tìm thấy Player với ID: {id}");
        return null;
    }

    // ==========================================
    // 1. ĐĂNG KÝ VÀ HỦY ĐĂNG KÝ EVENT
    // ==========================================
    private void OnEnable()
    {
        GameEvents.OnCharacterSelected += HandleCharacterSelection;
        GameEvents.OnHealthChanged += UpdatePlayerHealth;
        GameEvents.OnPlayerDied += HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected += StartMatchSequence;
        GameEvents.OnCoreSelectionFinished += HandleCoreSelectionFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnCharacterSelected -= HandleCharacterSelection;
        GameEvents.OnHealthChanged -= UpdatePlayerHealth;
        GameEvents.OnPlayerDied -= HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected -= StartMatchSequence;
        GameEvents.OnCoreSelectionFinished -= HandleCoreSelectionFinished;
    }

    // Hàm này sẽ được gọi khi có người chơi chọn xong nhân vật, nhận vào ID người chơi và ID nhân vật đã chọn
    private void HandleCharacterSelection(int playerID, int characterID)
    {
        // 1. Lưu ID nhân vật vào biến tương ứng
        if (playerID == 1)
        {
            player1CharacterID = characterID;
            Debug.Log($"Người chơi 1 đã chọn nhân vật: {characterID}");
        }
        else if (playerID == 2)
        {
            player2CharacterID = characterID;
            Debug.Log($"Người chơi 2 đã chọn nhân vật: {characterID}");
        }

        // 2. Kiểm tra xem cả hai đã chọn xong chưa
        CheckAllCharactersSelected();
    }

    // Hàm này kiểm tra nếu cả 2 người chơi đã chọn xong nhân vật chưa, nếu rồi thì chuyển sang bước tiếp theo
    private void CheckAllCharactersSelected()
    {
        // Nếu cả 2 ID đều khác -1, nghĩa là đã chọn xong
        if (player1CharacterID != -1 && player2CharacterID != -1)
        {
            Debug.Log("Tất cả người chơi đã chọn xong! Chuẩn bị vào trận...");

            // Chuyển trạng thái sang Loading hoặc Countdown
            GameEvents.RaiseAllCharactersSelected();
        }
    }

    // ==========================================
    // 2. LOGIC ĐIỀU HƯỚNG TRẠNG THÁI (STATE)
    // ==========================================
    private void ChangeState(GameState newState)
    {
        currentState = newState;
        GameEvents.RaiseGameStateChanged(newState); // Báo cho toàn game biết State đã đổi
    }

    // Cập nhật lại StartMatchSequence để reset máu mỗi hiệp
    public void StartMatchSequence()
    {
        ChangeState(GameState.RoundStarting);
        GameEvents.RaiseRoundStarted(currentRound);
        // Reset máu logic ở đây hoặc trong script Player
        StartCoroutine(CountdownRoutine());
    }

    // ==========================================
    // 3. VÒNG LẶP THỜI GIAN (TIMERS)
    // ==========================================
    private IEnumerator CountdownRoutine()
    {
        int count = 3;
        while (count > 0)
        {
            GameEvents.RaiseCountdownTick(count); // Báo UI hiện số 3, 2, 1
            yield return new WaitForSeconds(1f);
            count--;
        }

        // Bắt đầu trận đấu
        GameEvents.RaiseMatchStarted(); // Báo UI hiện chữ "FIGHT!"
        ChangeState(GameState.Fighting);

        // Chạy đồng hồ trận đấu
        StartCoroutine(MatchTimerRoutine());
    }

    private IEnumerator MatchTimerRoutine()
    {
        currentTime = matchDuration;

        while (currentTime > 0 && currentState == GameState.Fighting)
        {
            currentTime -= Time.deltaTime;
            GameEvents.RaiseTimerTick(currentTime); // Báo UI update số giây
            yield return null; // Chờ frame tiếp theo
        }

        // Nếu thoát vòng lặp mà thời gian <= 0, nghĩa là Hết giờ
        if (currentTime <= 0 && currentState == GameState.Fighting)
        {
            GameEvents.RaiseTimeOut();
            HandleTimeOut();
        }
    }

    // ==========================================
    // 4. XỬ LÝ DỮ LIỆU & QUYẾT ĐỊNH THẮNG THUA
    // ==========================================

    // Luôn cập nhật máu vào bụng GameManager để dành lúc hết giờ đem ra so sánh
    private void UpdatePlayerHealth(int playerID, int currentHP)
    {
        if (playerID == 1) player1HP = currentHP;
        else if (playerID == 2) player2HP = currentHP;
    }

    private void HandlePlayerDeath(int deadPlayerID)
    {
        if (currentState != GameState.Fighting) return;

        // Kết thúc hiệp chứ chưa kết thúc match — chuyển sang trạng thái RoundOver,
        // rồi AnnounceWinner sẽ dẫn sang CoreSelection nếu trận chưa kết thúc
        ChangeState(GameState.RoundOver);

        // Ai không chết thì người đó thắng
        int winnerID = (deadPlayerID == 1) ? 2 : 1;

        AnnounceWinner(winnerID);
    }

    private void HandleTimeOut()
    {
        // Hết giờ -> hiệp kết thúc nhưng chưa chắc là kết thúc cả trận
        ChangeState(GameState.RoundOver);

        int winnerID = 0; // Mặc định 0 là Hòa (Draw)

        if (player1HP > player2HP) winnerID = 1;
        else if (player2HP > player1HP) winnerID = 2;

        AnnounceWinner(winnerID);
    }

    private void AnnounceWinner(int winnerID)
    {
        // 1. Cộng điểm hiệp đấu
        if (winnerID == 1) p1RoundWins++;
        else if (winnerID == 2) p2RoundWins++;

        GameEvents.RaiseShowKO(winnerID);

        if (p1RoundWins < roundsToWin && p2RoundWins < roundsToWin)
        {
            // Thay vì Reset ngay, chúng ta chuyển sang luồng chọn lõi
            StartCoroutine(CoreSelectionFlow());
        }
        else
        {
            GameEvents.RaiseMatchEnded(winnerID);
        }
    }
    private IEnumerator CoreSelectionFlow()
    {
        yield return new WaitForSeconds(2f); // Thời gian chờ sau K.O
        ChangeState(GameState.CoreSelection);

        // Bắt đầu từ Player 1
        GameEvents.RaiseCoreSelectionStarted(1);
    }

    // Hàm này sẽ được gọi khi CoreUIHandler báo Player 2 đã chọn xong
    private void HandleCoreSelectionFinished()
    {
        StartNextRound();
    }
    public void StartNextRound()
    {
        currentRound++;
        //ResetPlayersPosition(); // Hàm tự viết để đưa 2 player về vị trí ban đầu
        StartMatchSequence();   // Gọi lại routine đếm ngược 3,2,1
    }

    private IEnumerator ShowWinScreenDelay(int winnerID)
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RaiseMatchEnded(winnerID);
        GameEvents.RaiseShowWinScreen(winnerID);
    }
}