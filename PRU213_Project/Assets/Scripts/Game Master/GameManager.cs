using System.Collections;
using UnityEngine;

// Định nghĩa GameState (Có thể vứt ra 1 file riêng cho sạch)
public enum GameState
{
    CharacterSelection,
    Loading,
    Countdown,
    Fighting,
    Paused,
    MatchOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Match Settings")]
    public int matchDuration = 90; // Thời gian 1 hiệp (giây)
    private float currentTime;

    [Header("State")]
    public GameState currentState; // Chữ thường, bỏ { get; private set; }

    // Lưu máu của 2 người chơi để check xem ai thắng nếu hết giờ
    private int player1HP;
    private int player2HP;

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

    // ==========================================
    // 1. ĐĂNG KÝ VÀ HỦY ĐĂNG KÝ EVENT
    // ==========================================
    private void OnEnable()
    {
        GameEvents.OnHealthChanged += UpdatePlayerHealth;
        GameEvents.OnPlayerDied += HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected += StartMatchSequence;
    }

    private void OnDisable()
    {
        GameEvents.OnHealthChanged -= UpdatePlayerHealth;
        GameEvents.OnPlayerDied -= HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected -= StartMatchSequence;
    }

    // ==========================================
    // 2. LOGIC ĐIỀU HƯỚNG TRẠNG THÁI (STATE)
    // ==========================================
    private void ChangeState(GameState newState)
    {
        currentState = newState;
        GameEvents.RaiseGameStateChanged(newState); // Báo cho toàn game biết State đã đổi
    }

    // Gọi hàm này khi load xong Scene Đánh nhau (Hoặc nghe từ event OnAllCharactersSelected)
    public void StartMatchSequence()
    {
        ChangeState(GameState.Countdown);
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

        ChangeState(GameState.MatchOver);

        // Ai không chết thì người đó thắng
        int winnerID = (deadPlayerID == 1) ? 2 : 1;

        AnnounceWinner(winnerID);
    }

    private void HandleTimeOut()
    {
        ChangeState(GameState.MatchOver);

        int winnerID = 0; // Mặc định 0 là Hòa (Draw)

        if (player1HP > player2HP) winnerID = 1;
        else if (player2HP > player1HP) winnerID = 2;

        AnnounceWinner(winnerID);
    }

    private void AnnounceWinner(int winnerID)
    {
        GameEvents.RaiseShowKO(winnerID); // Gọi UI đập chữ K.O vào mặt người chơi

        // Đợi 2 giây cho hiệu ứng K.O chạy xong rồi mới văng bảng Win Screen
        StartCoroutine(ShowWinScreenDelay(winnerID));
    }

    private IEnumerator ShowWinScreenDelay(int winnerID)
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RaiseMatchEnded(winnerID);
        GameEvents.RaiseShowWinScreen(winnerID);
    }
}