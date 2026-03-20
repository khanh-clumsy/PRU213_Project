using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    private Dictionary<int, PlayerRuntimeData> playerRuntimeStats = new Dictionary<int, PlayerRuntimeData>();
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
        // Luôn cập nhật để tránh tham chiếu cũ
        players[id] = playerScript;
        Debug.Log($"Player {id} đã đăng ký thành công!");
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

    /// <summary>
    /// Kiểm tra xem số hiệp (roundNumber) có hợp lệ không
    /// Hợp lệ: roundNumber phải từ 1 tới roundScenes.Length
    /// </summary>
    private bool IsValidRound(int roundNumber)
    {
        if (roundNumber < 1 || roundNumber > roundScenes.Length)
        {
            Debug.LogError($"Round {roundNumber} không hợp lệ! Chỉ có {roundScenes.Length} hiệp.");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Lưu trạng thái hiện tại của tất cả Player vào playerRuntimeStats
    /// Phải gọi hàm này TRƯỚC khi tải scene mới (sau ApplyCoreEffect chạy xong)
    /// </summary>
    public void SavePlayerStats()
    {
        playerRuntimeStats.Clear();

        foreach (var kvp in players)
        {
            int playerId = kvp.Key;
            Player player = kvp.Value;

            if (player == null) continue;

            // Lấy moveSpeed từ component PlayerMovement
            float moveSpeed = 0f;
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                moveSpeed = playerMovement.moveSpeed;
            }

            PlayerRuntimeData data = new PlayerRuntimeData(
                player.maxHP,
                player.CurrentHP,
                player.maxMana,
                player.CurrentMana,
                player.attackDamage,
                moveSpeed
            );

            playerRuntimeStats[playerId] = data;
            Debug.Log($"<color=green>[SaveStats]</color> Player {playerId} - HP: {player.CurrentHP}/{player.maxHP}, Mana: {player.CurrentMana}/{player.maxMana}, DMG: {player.attackDamage}, Speed: {moveSpeed}");
        }
    }

    /// <summary>
    /// Áp dụng dữ liệu đã lưu vào nhân vật mới trong scene mới
    /// Đảm bảo tất cả chỉ số (HP, Mana, Damage, Speed) được khôi phục
    /// </summary>
    private void ApplySavedStats(Player player)
    {
        if (player == null) return;

        if (playerRuntimeStats.TryGetValue(player.playerID, out PlayerRuntimeData data))
        {
            // Cập nhật chỉ số Player
            player.maxHP = data.maxHP;
            player.maxMana = data.maxMana;
            player.attackDamage = data.attackDamage;

            // Dùng setter để cập nhật currentHP và currentMana
            player.SetCurrentHP = data.currentHP;
            player.SetCurrentMana = data.currentMana;

            // Cập nhật moveSpeed cho PlayerMovement mới
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.moveSpeed = data.moveSpeed;
                Debug.Log($"<color=cyan>[ApplyStats - Speed]</color> Player {player.playerID} moveSpeed set to {data.moveSpeed}");
            }

            GameEvents.RaiseHealthChanged(player.playerID, player.CurrentHP);

            Debug.Log($"<color=cyan>[ApplyStats]</color> Player {player.playerID} - HP: {player.CurrentHP}/{player.maxHP}, Mana: {player.CurrentMana}/{player.maxMana}, DMG: {player.attackDamage}, Speed: {data.moveSpeed}");
        }
    }

    /// <summary>
    /// Cấu hình Camera động để theo sát cả 2 nhân vật
    /// Gọi hàm này sau khi SpawnPlayers() đã hoàn tất
    /// </summary>
    private void SetupCamera()
    {
        DynamicCameraController cam = FindObjectOfType<DynamicCameraController>();

        if (cam == null)
        {
            Debug.LogWarning("Không tìm thấy DynamicCameraController trong Scene!");
            return;
        }

        Player player1 = GetPlayer(1);
        Player player2 = GetPlayer(2);

        if (player1 != null && player2 != null)
        {
            cam.player1 = player1.transform;
            cam.player2 = player2.transform;
            Debug.Log($"<color=magenta>[SetupCamera]</color> Camera đã được cấu hình với Player 1 và Player 2");
        }
        else
        {
            Debug.LogWarning($"Không tìm thấy một hoặc cả hai Player! player1={player1}, player2={player2}");
        }
    }

    public void SpawnPlayers()
    {
        // Xóa các tham chiếu cũ đã bị destroy ở map trước
        players.Clear();

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

                // Đăng ký Player với GameManager
                RegisterPlayer(sp.playerID, controller);

                // Áp dụng dữ liệu đã lưu từ map trước
                ApplySavedStats(controller);
            }

            if (playerInputHandler != null)
            {
                // Gọi Initialize() để đặt playerType và bind controls
                PlayerInputHandler.PlayerType inputType = (sp.playerID == 1) 
                    ? PlayerInputHandler.PlayerType.Player1 
                    : PlayerInputHandler.PlayerType.Player2;
                playerInputHandler.Initialize(inputType);
            }

            // Setup Layer, Tag cho Player 2
            SetupPlayerLayers(playerObj, sp.playerID);

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

    /// <summary>
    /// Pause system is now handled by PauseManager.cs
    /// This method was removed to avoid conflicts with CoreUIHandler's Time.timeScale management
    /// </summary>

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

    /// <summary>
    /// Public wrapper để PauseManager có thể thay đổi state
    /// </summary>
    public void ChangeStatePublic(GameState newState)
    {
        ChangeState(newState);
    }

    /// <summary>
    /// Public wrapper để PauseManager có thể enable/disable actions
    /// </summary>
    public void SetAllPlayersActionsPublic(bool enable)
    {
        SetAllPlayersActions(enable);
    }

    // Cập nhật lại StartMatchSequence để reset máu mỗi hiệp
    public void StartMatchSequence()
    {
      //  GameEvents.RaiseScoreChanged(p1RoundWins, p2RoundWins); // hien ti so 0 - 0 khi bat dau vong dau tien
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
            GameEvents.RaiseCountdownTick(count); // UI hiện 3, 2, 1
            Debug.Log($"Đếm ngược: {count}");

            yield return new WaitForSeconds(1f);
            count--;
        }

        // Hiện chữ FIGHT!
        GameEvents.RaiseMatchStarted();
        Debug.Log("FIGHT!");

        // Chờ 1 giây để người chơi nhìn thấy chữ FIGHT!
        yield return new WaitForSeconds(1f);

        // Hiện tỉ số sau khi 3,2,1,FIGHT xong
        Debug.Log("[GameManager] RaiseScoreChanged");
        GameEvents.RaiseScoreChanged(p1RoundWins, p2RoundWins);
  


        // Sau khi countdown xong mới cho player hành động
        SetAllPlayersActions(true);
        Debug.Log("<color=green>[EnableActions]</color> Enabled all actions for fighting");

        // Chuyển state sang Fighting
        ChangeState(GameState.Fighting);

        // Bắt đầu đồng hồ trận đấu
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

        // Thêm delay 5 giây trước khi announce winner (cho hiệu ứng chết)
        StartCoroutine(AnnounceWinnerWithDelay(winnerID));
    }

    private IEnumerator AnnounceWinnerWithDelay(int winnerID)
    {
        yield return new WaitForSeconds(5f);
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
        GameEvents.RaiseScoreChanged(p1RoundWins, p2RoundWins);

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

        // Reset Mana cho cả 2 người chơi về 0 trước khi chọn Lõi
        Player player1 = GetPlayer(1);
        Player player2 = GetPlayer(2);

        if (player1 != null)
        {
            player1.ModifyCurrentMana(-player1.CurrentMana);
            Debug.Log($"<color=blue>[ResetMana]</color> Player 1 - Mana reset về 0");
        }

        if (player2 != null)
        {
            player2.ModifyCurrentMana(-player2.CurrentMana);
            Debug.Log($"<color=blue>[ResetMana]</color> Player 2 - Mana reset về 0");
        }

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
        if (!IsValidRound(roundNumber))
        {
            yield break;
        }

        // 2. Lấy tên scene tương ứng với hiệp (round 1 = index 0)
        string sceneName = roundScenes[roundNumber - 1];
        Debug.Log($"Đang tải scene: {sceneName} cho hiệp {roundNumber}...");

        // LƯU STATS: Lưu trạng thái nhân vật hiện tại TRƯỚC khi tải scene mới
        SavePlayerStats();

        // RESET HP: Nếu không phải hiệp 1, reset currentHP = maxHP trong dữ liệu lưu trữ
        if (roundNumber > 1)
        {
            foreach (var kvp in playerRuntimeStats)
            {
                kvp.Value.currentHP = kvp.Value.maxHP;
                Debug.Log($"<color=yellow>[ResetHP]</color> Player {kvp.Key} - Reset HP về {kvp.Value.maxHP}");
            }
        }

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

        // 6. Spawn người chơi (sẽ apply saved stats trong SpawnPlayers)
        SpawnPlayers();

        // 6.1. Chờ thêm 1 frame để Player.Start() và RegisterPlayer() chạy xong hoàn toàn
        yield return null;

        // 6.2. Disable actions TRƯỚC khi bắt đầu round (khi dict đã có player)
        SetAllPlayersActions(false);
        Debug.Log($"<color=orange>[DisableActions]</color> Disabled all actions before countdown");

        // 6.5. Cấu hình Camera động để theo sát cả 2 nhân vật
        SetupCamera();

        // 7. Bắt đầu hiệp (countdown 3,2,1 rồi đánh)
        StartMatchSequence();
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

    /// <summary>
    /// Tự động cấu hình Layer, Tag cho Player 2 sau khi Instantiate
    /// Player 1 giữ nguyên cấu hình từ prefab
    /// </summary>
    private void SetupPlayerLayers(GameObject playerObj, int playerID)
    {
        if (playerID == 1) return; // Player 1 giữ nguyên như prefab

        // 1. Set Layer cho root object
        playerObj.layer = LayerMask.NameToLayer("Player2");
        Debug.Log($"[Setup] Root object layer set to: Player2");

        // 2. Set Layer cho Hitbox (child)
        Transform hitbox = playerObj.transform.Find("Hitbox");
        if (hitbox != null)
        {
            hitbox.gameObject.layer = LayerMask.NameToLayer("Hitbox2");
            Debug.Log($"[Setup] Hitbox layer set to: Hitbox2");
        }
        else
        {
            Debug.LogWarning($"[Setup] Không tìm thấy child Hitbox trong Player {playerID}");
        }

        // 3. Set Layer cho Hurtbox (child)
        Transform hurtbox = playerObj.transform.Find("Hurtbox");
        if (hurtbox != null)
        {
            hurtbox.gameObject.layer = LayerMask.NameToLayer("Player2");
            Debug.Log($"[Setup] Hurtbox layer set to: Player2");
        }
        else
        {
            Debug.LogWarning($"[Setup] Không tìm thấy child Hurtbox trong Player {playerID}");
        }

        // 4. Set Layer cho GroundCheck (child)
        Transform groundCheck = playerObj.transform.Find("GroundCheck");
        if (groundCheck != null)
        {
            groundCheck.gameObject.layer = LayerMask.NameToLayer("Player2");
            Debug.Log($"[Setup] GroundCheck layer set to: Player2");
        }
        else
        {
            Debug.LogWarning($"[Setup] Không tìm thấy child GroundCheck trong Player {playerID}");
        }

        // 5. Cập nhật groundLayer mask trong Player script để IsGrounded() check đúng layer
        Player playerScript = playerObj.GetComponent<Player>();
        if (playerScript != null)
        {
            // groundLayer vẫn giữ nguyên vì cả 2 đều check layer "Ground"
            Debug.Log($"<color=cyan>[Setup]</color> Player2 layers đã được set thành công");
        }
    }

    /// <summary>
    /// Tự động enable/disable action của tất cả player theo GameState
    /// Chỉ cho phép điều khiển khi đang ở trạng thái Fighting
    /// </summary>
    private void SetAllPlayersActions(bool enable)
    {
        foreach (var kvp in players)
        {
            Player p = kvp.Value;
            if (p == null) continue;

            if (enable)
            {
                p.EnableAllActions();
                Debug.Log($"<color=green>[Actions]</color> Player {p.playerID} - Actions Enabled");
            }
            else
            {
                p.DisableAllActions();
                Debug.Log($"<color=orange>[Actions]</color> Player {p.playerID} - Actions Disabled");
            }
        }
    }
}