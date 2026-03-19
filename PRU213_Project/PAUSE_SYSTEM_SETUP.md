# 🎮 PAUSE SYSTEM - HƯỚNG DẪN SETUP

## 📋 Tổng Quan
Khi bấm **ESC**:
1. ✅ Game tạm dừng (Pause)
2. ✅ Khóa cử động của cả 2 player (disable actions)
3. ✅ Set trạng thái game thành `GameState.Paused`
4. ✅ Hiển thị Pause Menu với nút Resume/Settings/Exit

Khi bấm **ESC lại**:
1. ✅ Game tiếp tục (Resume)
2. ✅ Mở cử động của cả 2 player (enable actions)
3. ✅ Restore trạng thái game về `Fighting` (hoặc trạng thái trước pause)
4. ✅ Ẩn Pause Menu

---

## 🔧 CÁC THÀNH PHẦN ĐÃ TẠO

### 1. **PauseManager.cs** 
📍 Location: `Assets/Scripts/Game Master/PauseManager.cs`

**Chức năng:**
- Lắng nghe input ESC từ người chơi
- Gọi `PauseGame()` khi bấm ESC lần 1
- Gọi `ResumeGame()` khi bấm ESC lần 2
- Quản lý trạng thái `isPaused`

**Key Methods:**
```csharp
public void PauseGame()      // Pause game + khóa input + phát event OnGamePaused
public void ResumeGame()     // Resume game + mở input + phát event OnGameResumed
public bool IsPaused => ... // Kiểm tra trạng thái pause
```

### 2. **PauseUIHandler.cs**
📍 Location: `Assets/Scripts/UI/PauseUIHandler.cs`

**Chức năng:**
- Hiển thị/ẩn Pause Menu Panel
- Setup button listeners (Resume, Settings, Exit)
- Subscribe tới GameEvents.OnGamePaused/OnGameResumed
- Hiển thị menu khi pause, ẩn menu khi resume

**Key Methods:**
```csharp
private void ShowPauseMenu()  // Active pausePanel
private void HidePauseMenu()  // Deactive pausePanel
private void ExitGame()       // Exit to desktop
```

### 3. **GameManager.cs** (Updated)
📍 Location: `Assets/Scripts/Game Master/GameManager.cs`

**Các hàm mới:**
```csharp
public void ChangeStatePublic(GameState newState)  // Public wrapper
public void SetAllPlayersActionsPublic(bool enable) // Public wrapper
```

**Lý do:** PauseManager cần gọi `ChangeState()` và `SetAllPlayersActions()` từ bên ngoài

---

## 🎨 UNITY SETUP - BẬN CẦN LÀM

### **Bước 1: Tạo Pause UI Panel trong Canvas**

1. Mở Battle Scene (Map1, Map2, Map3)
2. Tìm hoặc tạo **Canvas**
3. Trong Canvas, tạo:
   - **PausePanel** (Panel image/background)
     - Màu đen semi-transparent (alpha ~0.8)
     - Anchor: Stretch (fill toàn screen)
     - Size: Full screen
   
   - **Pause Title** (Text)
     - Text: "PAUSED"
     - Font size: 80+
     - Position: Center top
   
   - **ResumeButton** (Button)
     - Text: "RESUME (ESC)"
     - Position: Center
   
   - **SettingsButton** (Button)
     - Text: "SETTINGS"
     - Position: Center bottom
   
   - **ExitButton** (Button)
     - Text: "EXIT GAME"
     - Position: Center bottom (dưới Settings)

### **Bước 2: Thêm PauseManager vào Scene**

1. Tạo Empty GameObject tên: **PauseManager**
2. Gắn script `PauseManager.cs` vào GameObject
3. Đặt ở bất kỳ vị trí nào trong Hierarchy (không quan trọng)

### **Bước 3: Thêm PauseUIHandler vào Canvas**

1. Tìm **Canvas** trong Hierarchy
2. Gắn script `PauseUIHandler.cs` vào Canvas
3. Trong Inspector, assign:
   - **pausePanel**: Drag PausePanel vào đây
   - **resumeButton**: Drag ResumeButton vào đây
   - **settingsButton**: Drag SettingsButton vào đây
   - **exitButton**: Drag ExitButton vào đây
   - **pauseTitlePanel** (optional): Drag Pause Title vào đây

### **Bước 4: Initial State - Ẩn PausePanel**

1. Select **PausePanel** trong Hierarchy
2. Gỡ check ở **Active** (hoặc SetActive(false) lúc runtime)
3. Lý do: Menu sẽ được show/hide bởi PauseUIHandler

---

## 📊 GAME FLOW

```
┌─────────────────────────────────────────┐
│         GAME RUNNING (Fighting)         │
└─────────────────────────────────────────┘
              ↓
         Player presses ESC
              ↓
    ┌────────────────────────┐
    │  PauseManager.Update() │
    │  Input.GetKeyDown(ESC) │
    │  → PauseGame()         │
    └────────────────────────┘
              ↓
    ┌────────────────────────────────┐
    │ 1. SetAllPlayersActions(false)  │ ← Khóa input
    │ 2. ChangeState(Paused)          │ ← Đổi state
    │ 3. RaiseGamePaused()            │ ← Phát event
    └────────────────────────────────┘
              ↓
    ┌────────────────────────────────┐
    │  PauseUIHandler.ShowPauseMenu() │
    │  pausePanel.SetActive(true)     │
    └────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│         GAME PAUSED (UI Shown)          │
│  [PAUSED]                               │
│  [RESUME] [SETTINGS] [EXIT]             │
└─────────────────────────────────────────┘
              ↓
   Player presses ESC hoặc clicks RESUME
              ↓
    ┌────────────────────────┐
    │  PauseManager.Update() │
    │  → ResumeGame()        │
    └────────────────────────┘
              ↓
    ┌────────────────────────────────┐
    │ 1. SetAllPlayersActions(true)   │ ← Mở input
    │ 2. ChangeState(Fighting)        │ ← Restore state
    │ 3. RaiseGameResumed()           │ ← Phát event
    └────────────────────────────────┘
              ↓
    ┌────────────────────────────────┐
    │  PauseUIHandler.HidePauseMenu() │
    │  pausePanel.SetActive(false)    │
    └────────────────────────────────┘
              ↓
┌─────────────────────────────────────────┐
│      GAME CONTINUES (Fighting Resume)   │
└─────────────────────────────────────────┘
```

---

## ⚙️ TÙNG CHỈNH THÊM (Optional)

### 1. **Bật Time.timeScale = 0 khi Pause**

Nếu bạn muốn thời gian thực sự dừng:

**PauseManager.cs - Bỏ comment:**
```csharp
public void PauseGame()
{
    // ...
    Time.timeScale = 0f;  // ← Uncomment dòng này
}

public void ResumeGame()
{
    // ...
    Time.timeScale = 1f;  // ← Uncomment dòng này
}
```

⚠️ **Cảnh báo:** Điều này sẽ ảnh hưởng tới **tất cả** Update/LateUpdate. Nếu UI animation bị dừng, cần dùng `UnscaledDeltaTime`.

### 2. **Add Sound Effects**

```csharp
// Trong PauseManager.cs
private AudioSource pauseSFX;

public void PauseGame()
{
    // ...
    pauseSFX.PlayOneShot(pauseSound); // Phát âm thanh pause
}
```

### 3. **Add Keyboard Shortcut Cho Resume**

```csharp
// Trong PauseUIHandler.cs
if (Input.GetKeyDown(KeyCode.Escape) && pauseManager.IsPaused)
{
    pauseManager.ResumeGame();
}
```

---

## 🐛 TROUBLESHOOTING

| Vấn Đề | Nguyên Nhân | Giải Pháp |
|--------|-----------|---------|
| Pause không hoạt động | PauseManager không được attach | Kiểm tra PauseManager có trong scene không |
| Menu không hiển thị | pausePanel reference thiếu | Drag PausePanel vào PauseUIHandler trong Inspector |
| Input vẫn hoạt động khi pause | SetAllPlayersActions(false) không gọi | Kiểm tra GameManager có SetAllPlayersActionsPublic không |
| Menu không ẩn khi resume | HidePauseMenu() không gọi | Kiểm tra OnGameResumed event có subscribe không |
| ESC không hoạt động | New Input System chưa setup | Chắc chắn Input.GetKeyDown(KeyCode.Escape) hoạt động |

---

## ✅ KIỂM TRA LIST

- [ ] PauseManager.cs được attach vào GameObject trong scene
- [ ] PauseUIHandler.cs được attach vào Canvas
- [ ] pausePanel được drag vào PauseUIHandler.pausePanel
- [ ] resumeButton được drag vào PauseUIHandler.resumeButton
- [ ] exitButton được drag vào PauseUIHandler.exitButton
- [ ] settingsButton được drag vào PauseUIHandler.settingsButton
- [ ] Pause Panel được ẩn lúc khởi động (SetActive(false))
- [ ] Build successful (không lỗi compile)
- [ ] Test: Bấm ESC → Menu hiển thị ✅
- [ ] Test: Bấm ESC lại → Menu ẩn, game tiếp tục ✅
- [ ] Test: Click Resume button → Game resume ✅
- [ ] Test: Player không thể điều khiển khi pause ✅

---

## 🎓 CÁC EVENTS LIÊN QUAN

```csharp
// GameEvents.cs - Pause events
public static event Action OnGamePaused;
public static void RaiseGamePaused() => OnGamePaused?.Invoke();

public static event Action OnGameResumed;
public static void RaiseGameResumed() => OnGameResumed?.Invoke();

public static event Action<GameState> OnGameStateChanged;
```

**Nếu bạn muốn subscribe tới pause events từ other systems:**
```csharp
private void OnEnable()
{
    GameEvents.OnGamePaused += HandlePause;
    GameEvents.OnGameResumed += HandleResume;
}

private void OnDisable()
{
    GameEvents.OnGamePaused -= HandlePause;
    GameEvents.OnGameResumed -= HandleResume;
}

private void HandlePause() { /* do something */ }
private void HandleResume() { /* do something */ }
```

---

## 📚 FILES CREATED

✅ **Assets/Scripts/Game Master/PauseManager.cs**
- Main pause/resume logic
- Input ESC detection

✅ **Assets/Scripts/UI/PauseUIHandler.cs**
- Pause menu UI display
- Button interactions

✅ **Assets/Scripts/Game Master/GameManager.cs** (Updated)
- Added `ChangeStatePublic()`
- Added `SetAllPlayersActionsPublic()`

---

**Setup hoàn tất! Bây giờ bạn có thể bấm ESC để pause game.** 🎮
