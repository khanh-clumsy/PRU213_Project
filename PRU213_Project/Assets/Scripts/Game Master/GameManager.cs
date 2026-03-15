using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Round & Scene Management")]
    public string[] roundScenes = new string[3] { "Map1", "Map2", "Map3" }; // Ánh xạ 3 hiệp với 3 scene

    [Header("Current Progress")]
    public int currentRound = 1;
    public int p1RoundWins = 0;
    public int p2RoundWins = 0;


    [Header("State & Time")]
    public GameState currentState; // Chữ thường, bỏ { get; private set; }
    private float currentTime;
    private bool isTimerRunning = false;


    [Header("Character Selection")]
    public int player1CharacterID = -1; // -1 nghĩa là chưa chọn
    public int player2CharacterID = -1;

    // Lưu máu của 2 người chơi để check xem ai thắng nếu hết giờ
    private int player1HP;
    private int player2HP;
    private Dictionary<int, Player> players = new Dictionary<int, Player>();

    [Header("Player Prefabs")]
    public GameObject[] characterPrefabs; // Danh sách các loại nhân vật bạn có

    // Core UI Handler reference
    private CoreUIHandler coreUI;

    private Coroutine currentMatchRoutine;

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

    /// <summary>
    /// Đăng ký CoreUIHandler để GameManager có thể tham chiếu khi cần
    /// Phương thức này được gọi bởi CoreUIHandler trong OnEnable()
    /// </summary>
    public void RegisterCoreUI(CoreUIHandler handler)
    {
        coreUI = handler;
        Debug.Log("CoreUIHandler đã đăng ký với GameManager");
    }

    public void SpawnPlayers()
    {
        // Tìm tất cả các điểm Spawn trong Scene hiện tại
        PlayerSpawn[] spawnPoints = FindObjectsOfType<PlayerSpawn>();

        foreach (PlayerSpawn sp in spawnPoints)
        {
            // 1. Lấy ID nhân vật đã chọn dựa trên playerID từ spawn point
            int selectedID = (sp.playerID == 1) ? player1CharacterID : player2CharacterID;

            // 2. Kiểm tra an toàn: ID phải hợp lệ và nằm trong phạm vi mảng
            if (selectedID < 0 || selectedID >= characterPrefabs.Length)
            {
                Debug.LogError($"Lỗi: ID nhân vật {selectedID} cho Player {sp.playerID} không hợp lệ hoặc chưa chọn!");
                continue;
            }

            GameObject prefabToSpawn = characterPrefabs[selectedID];

            // 3. Tạo nhân vật tại vị trí của điểm Spawn
            GameObject playerObj = Instantiate(prefabToSpawn, sp.transform.position, Quaternion.identity);

            // FIX BUG: Tắt GameObject ngay lập tức để tránh OnEnable() được kích hoạt
            // trước khi playerType được gán
            playerObj.SetActive(false);

            // 4. Thiết lập thông số ban đầu cho nhân vật
            var controller = playerObj.GetComponent<Player>();
            var playerInputHandler = playerObj.GetComponent<PlayerInputHandler>();

            if (controller != null)
            {
                controller.playerID = sp.playerID;
            }

            if (playerInputHandler != null)
            {
                // Gọi Initialize() để đặt playerType và bind controls
                PlayerInputHandler.PlayerType inputType = (sp.playerID == 1) 
                    ? PlayerInputHandler.PlayerType.Player1 
                    : PlayerInputHandler.PlayerType.Player2;
                playerInputHandler.Initialize(inputType);
            }

            // Xử lý quay mặt (Flip) nhân vật
            Vector3 localScale = playerObj.transform.localScale;
            if (sp.isFacingRight)
                localScale.x = Mathf.Abs(localScale.x); // Quay phải
            else
                localScale.x = -Mathf.Abs(localScale.x); // Quay trái

            playerObj.transform.localScale = localScale;

            // FIX BUG: Kích hoạt GameObject sau khi mọi thứ đã được cấu hình
            playerObj.SetActive(true);
        }
    }


    // ==========================================
    // 1. ĐĂNG KÝ VÀ HỦY ĐĂNG KÝ EVENT
    // ==========================================
    private void OnEnable()
    {
        GameEvents.OnCharacterSelected += HandleCharacterSelection;
        GameEvents.OnHealthChanged += UpdatePlayerHealth;
        GameEvents.OnPlayerDied += HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected += HandleAllCharactersSelected;
        GameEvents.OnCoreSelectionFinished += HandleCoreSelectionFinished;
    }

    private void OnDisable()
    {
        GameEvents.OnCharacterSelected -= HandleCharacterSelection;
        GameEvents.OnHealthChanged -= UpdatePlayerHealth;
        GameEvents.OnPlayerDied -= HandlePlayerDeath;
        GameEvents.OnAllCharactersSelected -= HandleAllCharactersSelected;
        GameEvents.OnCoreSelectionFinished -= HandleCoreSelectionFinished;
    }
    void Update()
    {
        // Kiểm tra nếu nhấn phím Escape (hoặc phím P tùy bạn)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Chỉ cho phép Pause khi đang trong trận đấu hoặc đang Pause rồi
            if (currentState == GameState.Fighting || currentState == GameState.Paused)
            {
                TogglePause();
            }
        }
    }
    public void TogglePause()
    {
        if (currentState == GameState.Fighting)
        {
            // Chuyển sang Pause
            Time.timeScale = 0f; // Dừng toàn bộ vật lý, animation và các hàm dùng DeltaTime
            ChangeState(GameState.Paused);
            GameEvents.RaiseGamePaused();
            Debug.Log("<color=orange>GAME PAUSED</color>");
        }
        else if (currentState == GameState.Paused)
        {
            // Tiếp tục game
            Time.timeScale = 1f; // Khôi phục tốc độ thời gian bình thường
            ChangeState(GameState.Fighting);
            GameEvents.RaiseGameResumed();
            Debug.Log("<color=green>GAME RESUMED</color>");
        }
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

    // Hàm này sẽ được gọi khi sự kiện OnAllCharactersSelected được phát ra
    // Nó sẽ kích hoạt quá trình tải cảnh cho hiệp 1
    private void HandleAllCharactersSelected()
    {
        StartCoroutine(LoadRoundScene(1));
    }

    // Hàm này kiểm tra nếu cả 2 người chơi đã chọn xong nhân vật chưa, nếu rồi thì chuyển sang bước tiếp theo
    private void CheckAllCharactersSelected()
    {
        // Nếu cả 2 ID đều khác -1, nghĩa là đã chọn xong
        if (player1CharacterID != -1 && player2CharacterID != -1)
        {
            Debug.Log("Tất cả người chơi đã chọn xong! Chuẩn bị vào trận...");
            // Sẽ được kích hoạt bởi sự kiện OnAllCharactersSelected
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
        if (currentMatchRoutine != null) StopCoroutine(currentMatchRoutine);
        currentMatchRoutine = StartCoroutine(CountdownRoutine());
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
            Debug.Log($"Đếm ngược: {count}");
            yield return new WaitForSeconds(1f);
            count--;
        }

        // Bắt đầu trận đấu
        GameEvents.RaiseMatchStarted(); // Báo UI hiện chữ "FIGHT!"
        ChangeState(GameState.Fighting);

        // Chạy đồng hồ trận đấu
        currentMatchRoutine = StartCoroutine(MatchTimerRoutine());
    }

    private IEnumerator MatchTimerRoutine()
    {
        currentTime = matchDuration;
        int lastLoggedSecond = -1;
        isTimerRunning = true;

        // Raise initial timer value
        GameEvents.RaiseTimerTick(currentTime);

        while (currentTime > 0 && (currentState == GameState.Fighting || currentState == GameState.Paused))
        {
            if (currentState == GameState.Fighting) // Chỉ trừ giờ khi đang Fighting
            {
                currentTime -= Time.deltaTime;

                // Clamp to 0 to avoid negative values
                if (currentTime < 0) currentTime = 0;

                // Raise timer event every frame
                GameEvents.RaiseTimerTick(currentTime);

                // Log every second for debugging
                int currentSecond = Mathf.CeilToInt(currentTime);
                if (currentSecond != lastLoggedSecond)
                {
                    lastLoggedSecond = currentSecond;
                    Debug.Log($"Thời gian trận đấu: {currentSecond} giây");
                }
            }
            yield return null;
        }

        // Mark timer as stopped
        isTimerRunning = false;

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
        else
        {
            Debug.Log("Hết giờ nhưng cả 2 người chơi đều còn máu bằng nhau! Hiệp đấu này là Hòa!");
        }
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
            StartCoroutine(CoreSelectionFlow());
        }
        else
        {
            ChangeState(GameState.MatchOver);
            StartCoroutine(ShowWinScreenDelay(winnerID));
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
        Debug.Log($"Bắt đầu hiệp {currentRound}!");

        // Tải scene cho hiệp tiếp theo
        StartCoroutine(LoadRoundScene(currentRound));
    }

    private IEnumerator LoadRoundScene(int roundNumber)
    {
        // 1. Kiểm tra xem roundNumber có hợp lệ không
        if (roundNumber < 1 || roundNumber > roundScenes.Length)
        {
            Debug.LogError($"Round {roundNumber} không hợp lệ! Chỉ có {roundScenes.Length} hiệp.");
            yield break;
        }

        // 2. Lấy tên scene tương ứng với hiệp (round 1 = index 0)
        string sceneName = roundScenes[roundNumber - 1];
        Debug.Log($"Đang tải scene: {sceneName} cho hiệp {roundNumber}...");

        // 3. Tải scene một cách không đồng bộ (async)
        // Tham số loadSceneMode = Single sẽ unload scene hiện tại
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        // 4. Chờ cho tới khi scene tải xong
        while (!asyncLoad.isDone)
        {
            Debug.Log($"Tiến trình tải: {asyncLoad.progress * 100f}%");
            yield return null;
        }

        Debug.Log($"Scene {sceneName} đã tải xong!");

        // 5. Chờ một frame để đảm bảo scene hoàn toàn sẵn sàng
        yield return null;

        // 6. Reset trạng thái người chơi (nếu không phải hiệp 1)
        if (roundNumber > 1)
        {
            ResetPlayersForNewRound();
        }

        // 7. Spawn người chơi
        SpawnPlayers();

        // 8. Bắt đầu hiệp (countdown 3,2,1 rồi đánh)
        StartMatchSequence();
    }

    private void ResetPlayersForNewRound()
    {
        // Reset HP về max
        Player player1 = GetPlayer(1);
        Player player2 = GetPlayer(2);

        if (player1 != null)
        {
            player1.ResetHealth();
            Debug.Log($"Player 1 reset HP: {player1.CurrentHP}/{player1.maxHP}");
        }

        if (player2 != null)
        {
            player2.ResetHealth();
            Debug.Log($"Player 2 reset HP: {player2.CurrentHP}/{player2.maxHP}");
        }
    }

    private IEnumerator ShowWinScreenDelay(int winnerID)
    {
        yield return new WaitForSeconds(2f);
        GameEvents.RaiseMatchEnded(winnerID);
        GameEvents.RaiseShowWinScreen(winnerID);
    }

    // ==========================================
    // 5. TIMER HELPER FUNCTIONS
    // ==========================================

    /// <summary>
    /// Gets the current remaining time in the match (in seconds)
    /// </summary>
    public float GetRemainingTime() => currentTime;

    /// <summary>
    /// Gets the maximum duration of a round (in seconds)
    /// </summary>
    public float GetMatchDuration() => matchDuration;

    /// <summary>
    /// Gets the timer progress as a normalized value (0 to 1)
    /// 1.0 = full time, 0.0 = no time left
    /// </summary>
    public float GetTimerProgress() => Mathf.Clamp01(currentTime / matchDuration);

    /// <summary>
    /// Checks if the timer is currently running
    /// </summary>
    public bool IsTimerRunning() => isTimerRunning;

    /// <summary>
    /// Stops the timer immediately (used for round end)
    /// </summary>
    private void StopTimer()
    {
        isTimerRunning = false;
        if (currentMatchRoutine != null)
        {
            StopCoroutine(currentMatchRoutine);
            currentMatchRoutine = null;
        }
    }

    /// <summary>
    /// Resets the timer to match duration (used for new rounds)
    /// </summary>
    private void ResetTimer()
    {
        currentTime = matchDuration;
        isTimerRunning = false; 
    }
}