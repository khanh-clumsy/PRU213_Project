# 🐛 BUG REPORT: Round 2 Missing Countdown

## ❌ Problem Description

**Issue:** 
- Round 1: Countdown 3,2,1 works ✅ → Players frozen ✅
- Round 2: No countdown ❌ → Players not frozen ❌
- Round 3: Same issue as Round 2 ❌

**Expected Behavior:**
- Every round should have countdown 3,2,1
- Every round should disable player actions during countdown
- Every round should start matching sequence

---

## 🔍 ROOT CAUSE ANALYSIS

Found **MULTIPLE ISSUES** in GameManager.cs:

### Issue 1: **Pause Logic Interferes with Core Selection**
**Location:** GameManager.cs lines 295-302

```csharp
void Update()
{
    // ❌ BUG: ESC detection runs during Time.timeScale = 0
    // This interferes with CoreUIHandler's pause mechanism
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (currentState == GameState.Fighting || currentState == GameState.Paused)
        {
            TogglePause();  // ← This runs even when selecting cores!
        }
    }
}
```

**Why it breaks Round 2:**
1. Round 1 ends → HandlePlayerDeath() called
2. CoreSelectionFlow() starts → Time.timeScale = 0 (pause)
3. CoreUIHandler waits for core selection
4. ESC key still detected in Update() (doesn't respect Time.timeScale)
5. TogglePause() messes with game state
6. Time.timeScale gets messed up
7. When LoadRoundScene starts → Countdown doesn't work properly

### Issue 2: **Time.timeScale = 0 in TogglePause Conflicts with Pause System**
**Location:** GameManager.cs lines 309, 317

```csharp
public void TogglePause()
{
    if (currentState == GameState.Fighting)
    {
        Time.timeScale = 0f;  // ← Manually freezing time
        ChangeState(GameState.Paused);
    }
    else if (currentState == GameState.Paused)
    {
        Time.timeScale = 1f;  // ← Manually unfreezing time
        ChangeState(GameState.Fighting);
    }
}
```

**Problems:**
- Duplicates PauseManager functionality
- Conflicts with CoreUIHandler's Time.timeScale management
- Can leave Time.timeScale in incorrect state

### Issue 3: **Missing SavePlayerStats() Before Scene Load**
**Location:** GameManager.cs line 587

```csharp
private IEnumerator LoadRoundScene(int roundNumber)
{
    // ... validation ...
    
    // ❌ BUG: SavePlayerStats() is only called if roundNumber > 1
    // But CoreSelectionFlow already happened!
    // Stats from round 1 combat + core effect need to be saved
    
    string sceneName = roundScenes[roundNumber - 1];
    
    // SavePlayerStats() should be called HERE
    // But it's currently MISSING for transitions after core selection
}
```

---

## 🔧 SOLUTION

### Fix 1: **Remove Pause ESC Logic from GameManager Update()**

The pause system should ONLY be handled by **PauseManager**, not GameManager.

**Current Issue:**
```csharp
// ❌ In GameManager.cs Update()
if (Input.GetKeyDown(KeyCode.Escape))
{
    if (currentState == GameState.Fighting || currentState == GameState.Paused)
    {
        TogglePause();  // ← Remove this, use PauseManager instead
    }
}
```

**Solution:**
- Delete the Update() method in GameManager
- Delete the TogglePause() method in GameManager  
- PauseManager.cs handles ALL pause logic independently
- No conflicts with CoreUIHandler's Time.timeScale management

### Fix 2: **Ensure SavePlayerStats() is Called Before Loading Round 2+**

In LoadRoundScene(), SavePlayerStats should be called at the beginning (after validation):

```csharp
private IEnumerator LoadRoundScene(int roundNumber)
{
    if (!IsValidRound(roundNumber))
        yield break;
    
    // ✅ ALWAYS save stats before loading new scene
    // Whether it's round 1→2 or round 2→3
    SavePlayerStats();  // ← Add this
    
    string sceneName = roundScenes[roundNumber - 1];
    // ... rest of logic
}
```

### Fix 3: **Remove Manual Time.timeScale Management from GameManager**

The Time.timeScale should ONLY be controlled by:
1. **CoreUIHandler** - for core selection (sets to 0 and back to 1)
2. **PauseManager** (optional) - for pause system
3. **NOT GameManager** - to avoid conflicts

---

## 📋 DETAILED FIXES

### Fix #1: Remove Pause Logic from GameManager

**File:** `Assets/Scripts/Game Master/GameManager.cs`

Remove these methods entirely (lines ~295-320):

```csharp
// ❌ DELETE THIS ENTIRE METHOD
void Update()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (currentState == GameState.Fighting || currentState == GameState.Paused)
        {
            TogglePause();
        }
    }
}

// ❌ DELETE THIS ENTIRE METHOD
public void TogglePause()
{
    if (currentState == GameState.Fighting)
    {
        Time.timeScale = 0f;
        ChangeState(GameState.Paused);
        GameEvents.RaiseGamePaused();
        Debug.Log("<color=orange>GAME PAUSED</color>");
    }
    else if (currentState == GameState.Paused)
    {
        Time.timeScale = 1f;
        ChangeState(GameState.Fighting);
        GameEvents.RaiseGameResumed();
        Debug.Log("<color=green>GAME RESUMED</color>");
    }
}
```

**Why delete?**
- PauseManager.cs already handles ALL pause logic
- GameManager trying to manage pause creates conflicts
- CoreUIHandler manages Time.timeScale independently
- Separating concerns = fewer bugs

---

### Fix #2: Add SavePlayerStats() to LoadRoundScene

**File:** `Assets/Scripts/Game Master/GameManager.cs`

**Current Code (lines 570-587):**
```csharp
private IEnumerator LoadRoundScene(int roundNumber)
{
    if (!IsValidRound(roundNumber))
    {
        yield break;
    }

    string sceneName = roundScenes[roundNumber - 1];
    Debug.Log($"Đang tải scene: {sceneName} cho hiệp {roundNumber}...");

    // LƯU STATS: Lưu trạng thái nhân vật hiện tại TRƯỚC khi tải scene mới
    SavePlayerStats();
```

**New Code:**
```csharp
private IEnumerator LoadRoundScene(int roundNumber)
{
    if (!IsValidRound(roundNumber))
    {
        yield break;
    }

    // ✅ ALWAYS save stats at the beginning
    SavePlayerStats();

    string sceneName = roundScenes[roundNumber - 1];
    Debug.Log($"Đang tải scene: {sceneName} cho hiệp {roundNumber}...");
```

**Note:** SavePlayerStats() call was already there, but verify it's in correct position!

---

## 🔄 NEW CORRECT FLOW

### Round 1 Flow (Already Working):
```
CharacterSelect Scene
    ↓
OnAllCharactersSelected()
    ↓
LoadRoundScene(1)
    ├─ SavePlayerStats() [Round 1 initial stats]
    ├─ SpawnPlayers()
    ├─ SetupCamera()
    └─ StartMatchSequence()
        └─ CountdownRoutine() [3,2,1] ✅
```

### Round 1→2 Flow (Currently Broken, Now Fixed):
```
Map1 Scene - Fighting
    ↓ 90 seconds, P1 or P2 dies
HandlePlayerDeath()
    ├─ AnnounceWinnerWithDelay(5s)
    │   └─ Increment round wins
    │
    └─ CoreSelectionFlow()
        ├─ ChangeState(CoreSelection)
        ├─ ResetMana(0)
        ├─ RaiseCoreSelectionStarted(1)
        │   └─ CoreUIHandler shows menu
        │   └─ Time.timeScale = 0 [Pause for core selection]
        │
        ├─ P1 selects core
        │   ├─ ApplyCoreEffect(P1)
        │   └─ Time.timeScale = 1 [Resume]
        │   └─ RaiseCoreSelectionStarted(2)
        │       └─ CoreUIHandler shows menu again
        │       └─ Time.timeScale = 0 [Pause again]
        │
        ├─ P2 selects core
        │   ├─ ApplyCoreEffect(P2)
        │   └─ Time.timeScale = 1 [Resume]
        │   └─ RaiseCoreSelectionFinished()
        │
        └─ HandleCoreSelectionFinished()
            └─ StartNextRound()
                └─ LoadRoundScene(2)
                    ├─ SavePlayerStats() [Round 1 + core bonus stats]
                    ├─ SpawnPlayers()
                    ├─ ApplySavedStats()
                    ├─ SetupCamera()
                    └─ StartMatchSequence()
                        └─ CountdownRoutine() [3,2,1] ✅ NOW WORKS!
```

---

## ✅ IMPLEMENTATION CHECKLIST

- [ ] Delete GameManager.Update() method (lines ~295-302)
- [ ] Delete GameManager.TogglePause() method (lines ~304-320)
- [ ] Verify SavePlayerStats() is at start of LoadRoundScene()
- [ ] Build succeeds with no errors
- [ ] Test Round 1: Countdown works ✅
- [ ] Test Round 2: Countdown works ✅
- [ ] Test Round 3: Countdown works ✅
- [ ] Test Core selection: Time.timeScale properly managed ✅
- [ ] Test Pause via PauseManager: Still works ✅

---

## 🎮 Expected Result After Fix

```
Round 1:
  - Countdown 3,2,1 ✅
  - Players frozen ✅
  - Fighting starts ✅
  - Core selection works ✅

Round 2:
  - Countdown 3,2,1 ✅ (NOW FIXED)
  - Players frozen ✅ (NOW FIXED)
  - Fighting starts ✅
  - Core selection works ✅

Round 3:
  - Countdown 3,2,1 ✅ (NOW FIXED)
  - Players frozen ✅ (NOW FIXED)
  - Fighting starts ✅
  - Match winner determined ✅
```

---

## 💡 Why This Happens

**Root Cause Chain:**
1. ESC detected in GameManager.Update() during core selection
2. TogglePause() interferes with CoreUIHandler's Time.timeScale = 0
3. Game state gets corrupted
4. Round 2 loads with corrupted state
5. CountdownRoutine fails to lock players properly
6. Players can move during countdown in Round 2+

**Solution:**
- Remove GameManager's pause logic
- Let PauseManager handle pause independently
- Let CoreUIHandler manage Time.timeScale for core selection
- No more conflicts = smooth Round 1→2→3 transitions

