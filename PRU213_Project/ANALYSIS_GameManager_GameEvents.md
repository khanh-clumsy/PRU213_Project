# 📋 PHÂN TÍCH GameManager.cs & GameEvents.cs

---

## 1️⃣ GameEvents.cs - Event System (Pub-Sub Pattern)

### 🎯 Mục Đích:
- **Hệ thống thông báo sự kiện** cho toàn game
- Các class có thể subscribe vào event và nhận thông báo mà không cần tham chiếu trực tiếp
- Giảm coupling giữa các component

### 📦 Các Event Chính:

| Event | Mục Đích | Callback |
|-------|---------|----------|
| `OnGameStateChanged` | Thay đổi trạng thái game | `RaiseGameStateChanged(GameState)` |
| `OnTimerTick` | Cập nhật thời gian còn lại | `RaiseTimerTick(float)` |
| `OnHealthChanged` | HP thay đổi → UI cập nhật | `RaiseHealthChanged(playerID, hp)` |
| `OnManaChanged` | Mana thay đổi → UI cập nhật | `RaiseManaChanged(playerID, mana)` |
| `OnPlayerDied` | Khi player chết | `RaisePlayerDied(playerID)` |
| `OnCountdownTick` | Countdown 3,2,1 | `RaiseCountdownTick(count)` |
| `OnCoreSelectionStarted` | Bắt đầu chọn Core | `RaiseCoreSelectionStarted(playerID)` |
| `OnGamePaused/Resumed` | Pause/Resume | `RaiseGamePaused/Resumed()` |

### ⚠️ Vấn Đề Hiện Tại:
```csharp
// ❌ NẾU QUÊN gọi RaiseManaChanged(), UI mana bar KHÔNG cập nhật
// ✅ PHẢI gọi RaiseManaChanged() ở AddMana, UseMana, ModifyCurrentMana
```

---

## 2️⃣ GameManager.cs - Orchestrator (Bộ điều phối chính)

### 🎯 Mục Đích:
- **Quản lý toàn bộ game flow** từ lựa chọn character → các hiệp đấu → kết thúc
- **Lưu trữ và khôi phục stats** player khi chuyển scene
- **Quản lý state machine** của game
- **Điều khiển timer, camera, spawn**

### 🏗️ Kiến Trúc:

```
CharacterSelection Scene
    ↓
Character Select → OnAllCharactersSelected
    ↓ (LoadRoundScene 1)
Map1 Scene (Round 1)
    ├─ SpawnPlayers()
    ├─ SetupCamera()
    ├─ CountdownRoutine() (3,2,1)
    ├─ MatchTimerRoutine() (90s fighting)
    └─ Player Dies/Timeout → AnnounceWinner()
    ↓
CoreSelectionFlow (Chọn Core để boost stat)
    ├─ Reset Mana → 0
    ├─ CoreUIHandler.OpenSelectionForPlayer()
    └─ OnCoreSelectionFinished()
    ↓
LoadRoundScene 2 (HP reset, stats persist)
    └─ (lặp lại...)
    ↓
Map3 Scene (Round 3)
    └─ Ai thắng 2 hiệp trước → Match Over
```

---

## 3️⃣ Các Hàm Quan Trọng Cần Lưu Ý

### 🔴 **SavePlayerStats()** - QUAN TRỌNG
```csharp
// Lưu stats TRƯỚC khi unload scene
// PHẢI gọi trước LoadSceneAsync()
// Nếu không → stats mất hết lúc chuyển map!
SavePlayerStats();
```

### 🟢 **ApplySavedStats()** - QUAN TRỌNG
```csharp
// Phục hồi stats từ bản lưu
// PHẢI gọi ngay sau RegisterPlayer()
// Nếu không → player bắt đầu với stats default
ApplySavedStats(player);
```

### 🟡 **SetupPlayerLayers()** - QUAN TRỌNG
```csharp
// Tự động set Layer cho Player 2 (phải trước SetActive)
// Nếu không → Hitbox collision sai
// Player1 = Hitbox / Player2 = Hitbox2 (khác nhau!)
SetupPlayerLayers(playerObj, sp.playerID);
```

### 🟠 **SetAllPlayersActions()** - QUAN TRỌNG
```csharp
// Enable/Disable input của player theo state
// SetAllPlayersActions(true)  → cho phép điều khiển
// SetAllPlayersActions(false) → chặn điều khiển
// Nếu không → player vẫn điều khiển lúc countdown!
```

### 🔵 **LoadRoundScene()** - CÓ BUG CẦN LƯU Ý
```csharp
// THỨ TỰ rất quan trọng:
SpawnPlayers();              // 1. Spawn + RegisterPlayer()
yield return null;           // 2. Chờ Player.Start() chạy xong
SetAllPlayersActions(false); // 3. Disable input (dict có data)
SetupCamera();              // 4. Setup camera
StartMatchSequence();       // 5. Countdown

// ❌ SAI: Gọi disable trước spawn → dict rỗng → không set được
// ✅ ĐÚNG: Gọi disable sau spawn + 1 frame
```

### 📍 **ChangeState()** - Đơn Giản Nhưng Quan Trọng
```csharp
// Thay đổi state + phát event
// UI/System sẽ subscribe vào OnGameStateChanged
// Không có logic phức tạp - chỉ là thông báo
ChangeState(GameState.Fighting);
```

---

## 4️⃣ Các Vấn Đề & Giải Pháp

### ❌ VẤN ĐỀ 1: Stats Mất Khi Chuyển Scene
**Nguyên Nhân:**
- Không gọi `SavePlayerStats()` trước `LoadSceneAsync()`
- Old player bị destroy, stats lẻ loi

**Giải Pháp:**
```csharp
LoadRoundScene():
    ↓ SavePlayerStats() TRƯỚC LoadSceneAsync
    ↓ LoadSceneAsync (scene unload)
    ↓ SpawnPlayers() → ApplySavedStats() NGAY SAU
```

### ❌ VẤN ĐỀ 2: Player Vẫn Điều Khiển Khi Countdown
**Nguyên Nhân:**
- Gọi `SetAllPlayersActions(false)` trước khi `players` dict có data
- Dict rỗng → không disable được

**Giải Pháp:**
```csharp
SpawnPlayers();           // Dict có data
yield return null;        // Chờ 1 frame
SetAllPlayersActions(false); // Giờ mới disable, dict không rỗng
```

### ❌ VẤN ĐỀ 3: DeadState Animation Bị Đè
**Nguyên Nhân:**
- PlayerStateMachine.ChangeState() không guard DeadState
- Các state khác vẫn có thể override

**Giải Pháp:**
```csharp
// Thêm guard vào ChangeState:
if (CurrentState is DeadState) return; // Khóa DeadState
```

### ❌ VẤN ĐỀ 4: HitBox Collision Sai Cho Player 2
**Nguyên Nhân:**
- Không set Layer khác cho Player 2
- Player1 Hitbox và Player2 Hurtbox cùng layer → collision sai

**Giải Pháp:**
```csharp
SetupPlayerLayers():
    Player1: Hitbox / Hurtbox
    Player2: Hitbox2 / Player2  // Khác nhau
    Physics2D Layer Matrix: Hitbox ↔ Hurtbox2, Hitbox2 ↔ Hurtbox
```

---

## 5️⃣ Tối Ưu Hóa Đề Xuất

### 1. **Tách GameState ra file riêng**
```csharp
// ❌ Hiện tại trong GameManager.cs
public enum GameState { ... }

// ✅ Nên tách ra:
// Assets/Scripts/Core/GameState.cs
public enum GameState { ... }
```
**Lợi Ích:** Code sạch, dễ manage

### 2. **Tách PlayerRuntimeData ra file riêng**
```csharp
// ❌ Hiện tại trong GameManager.cs
public class PlayerRuntimeData { ... }

// ✅ Nên tách ra:
// Assets/Scripts/Core/PlayerRuntimeData.cs
[System.Serializable]
public class PlayerRuntimeData { ... }
```
**Lợi Ích:** Tái sử dụng, test riêng

### 3. **Thêm Method ValidateRoundNumber()**
```csharp
// ❌ Lặp logic kiểm tra trong LoadRoundScene:
if (roundNumber < 1 || roundNumber > roundScenes.Length)
{
    Debug.LogError(...);
    yield break;
}

// ✅ Extract ra method riêng:
private bool IsValidRound(int roundNumber)
{
    if (roundNumber < 1 || roundNumber > roundScenes.Length)
    {
        Debug.LogError($"Round {roundNumber} không hợp lệ");
        return false;
    }
    return true;
}
```

### 4. **Cache FindObjectOfType() Results**
```csharp
// ❌ Gọi lại mỗi lần cần (chậm):
DynamicCameraController cam = FindObjectOfType<DynamicCameraController>();

// ✅ Cache nó:
private DynamicCameraController cachedCamera;
private void SetupCamera()
{
    if (cachedCamera == null)
        cachedCamera = FindObjectOfType<DynamicCameraController>();
}
```

### 5. **Thêm Getter cho TimerProgress**
```csharp
// ✅ Có rồi nhưng tốt lắm:
public float GetTimerProgress() => Mathf.Clamp01(currentTime / matchDuration);
// Dùng để fill progress bar
```

### 6. **Guard Null Check Cho Players Dictionary**
```csharp
// ❌ Có thể lỗi NullReference:
foreach (var kvp in players) { ... }

// ✅ Thêm guard:
if (players == null || players.Count == 0)
{
    Debug.LogWarning("Players dictionary is empty!");
    return;
}
foreach (var kvp in players) { ... }
```

### 7. **Consolidate Log Statements**
```csharp
// Có quá nhiều Debug.Log với màu khác nhau
// Có thể tạo Debug Helper class:
public static class GameDebug
{
    public static void LogSaveStats(string message) 
        => Debug.Log($"<color=green>[SaveStats]</color> {message}");
    
    public static void LogActions(string message) 
        => Debug.Log($"<color=orange>[Actions]</color> {message}");
}
// Dùng: GameDebug.LogSaveStats("...");
```

---

## 6️⃣ Diagram Flow Chi Tiết

```
┌─────────────────────────────────────────────────────────────┐
│              GAME INITIALIZATION                            │
├─────────────────────────────────────────────────────────────┤
│ Awake() → Singleton Pattern                                │
│ OnEnable() → Subscribe Events (Character, Health, Death)   │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│        CHARACTER SELECTION SCENE                            │
├─────────────────────────────────────────────────────────────┤
│ Player 1 & 2 select character                              │
│ → OnAllCharactersSelected → LoadRoundScene(1)              │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│        ROUND X FLOW (Map1/Map2/Map3)                        │
├─────────────────────────────────────────────────────────────┤
│ SavePlayerStats() (nếu round > 1)                          │
│ ↓                                                            │
│ LoadSceneAsync(MapX)                                        │
│ ↓                                                            │
│ SpawnPlayers() + yield null + SetupCamera()                │
│ ↓                                                            │
│ SetAllPlayersActions(false) [disable input]                │
│ ↓                                                            │
│ StartMatchSequence()                                        │
│ ├─ CountdownRoutine() (3,2,1)                              │
│ │  ↓                                                        │
│ │  SetAllPlayersActions(true) [enable input]               │
│ │  ↓                                                        │
│ │  ChangeState(Fighting)                                   │
│ │                                                           │
│ └─ MatchTimerRoutine() (90 seconds)                        │
│    ├─ Update timer every frame                            │
│    └─ OnTimeOut → HandleTimeOut()                         │
│                                                            │
│ Combat Events:                                            │
│ ├─ OnHitConfirmed → AddMana(20)                           │
│ ├─ OnHealthChanged → Update HP bar                        │
│ ├─ OnPlayerDied → AnnounceWinnerWithDelay(5s)             │
│ └─ OnTimeOut → Compare HP → Winner                        │
│                                                            │
│ Winner Announcement:                                       │
│ ├─ RaiseShowKO()                                          │
│ └─ If winner needed more → CoreSelectionFlow()            │
│    Else → MatchOver → ShowWinScreen()                      │
└─────────────────────────────────────────────────────────────┘
                          ↓
┌─────────────────────────────────────────────────────────────┐
│        CORE SELECTION FLOW (if not match over)             │
├─────────────────────────────────────────────────────────────┤
│ CoreSelectionFlow():                                       │
│ ├─ ResetMana(P1) → 0                                      │
│ ├─ ResetMana(P2) → 0                                      │
│ ├─ ChangeState(CoreSelection)                            │
│ └─ RaiseCoreSelectionStarted(1)                           │
│                                                            │
│ P1 Chọn Core → ApplyCoreEffect(P1)                        │
│ ↓ RaiseCoreSelectionStarted(2)                            │
│                                                            │
│ P2 Chọn Core → ApplyCoreEffect(P2)                        │
│ ↓ RaiseCoreSelectionFinished()                            │
│                                                            │
│ SavePlayerStats() (lưu bonus từ core)                     │
│ StartNextRound() → currentRound++                         │
│ LoadRoundScene(currentRound)                              │
└─────────────────────────────────────────────────────────────┘
                          ↓
        [Quay lại ROUND X FLOW với round mới]
                          ↓
        [Lặp lại cho tới khi có winner]
                          ↓
┌─────────────────────────────────────────────────────────────┐
│        MATCH OVER                                          │
├─────────────────────────────────────────────────────────────┤
│ p1RoundWins >= roundsToWin OR p2RoundWins >= roundsToWin  │
│ ↓                                                           │
│ ChangeState(MatchOver)                                     │
│ ShowWinScreenDelay(winnerID)                              │
│ RaiseShowWinScreen(winnerID)                              │
│ RaiseMatchEnded(winnerID)                                 │
└─────────────────────────────────────────────────────────────┘
```

---

## 7️⃣ Checklist Khi Sửa Code

✅ **Trước khi thay đổi stats:**
- [ ] Gọi `SavePlayerStats()` 
- [ ] Reset HP nếu round > 1
- [ ] Gọi `ApplySavedStats()` sau spawn

✅ **Khi spawn player:**
- [ ] SetActive(false) TRƯỚC cấu hình
- [ ] RegisterPlayer() 
- [ ] ApplySavedStats()
- [ ] SetupPlayerLayers()
- [ ] SetActive(true) CUỐI

✅ **Khi disable/enable input:**
- [ ] Đảm bảo `players` dict có data
- [ ] Gọi `SetAllPlayersActions()` SAU spawn + 1 frame

✅ **Khi phát event:**
- [ ] Chọn event đúng loại
- [ ] Truyền đúng parameter
- [ ] Không quên phát event!

---

## 🎓 Kết Luận

**GameManager.cs** là trái tim của game, quản lý toàn bộ flow.
**GameEvents.cs** là hệ thần kinh, truyền thông tin giữa các component.

Chúng hoạt động như một máy bay:
- **GameManager** = Pilot điều khiển
- **GameEvents** = Radio liên lạc giữa các phòng
- **Player/UI** = Passengers nhận thông tin

Nếu một trong những hàm quan trọng (SaveStats, ApplyStats, SetupLayers, SetAllActions) bị sai **thứ tự**, cả game sẽ **hỏng**.

