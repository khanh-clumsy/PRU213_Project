using System;
public static class GameEvents

{

    // =====================================================

    // GAME FLOW

    // =====================================================



    public static event Action<GameState> OnGameStateChanged;

    public static void RaiseGameStateChanged(GameState newState)

        => OnGameStateChanged?.Invoke(newState);



    public static event Action<int> OnCountdownTick; // 3,2,1

    public static void RaiseCountdownTick(int count)

        => OnCountdownTick?.Invoke(count);



    public static event Action OnMatchStarted;

    public static void RaiseMatchStarted()

        => OnMatchStarted?.Invoke();



    public static event Action<int> OnMatchEnded; // winnerId (0 = draw)

    public static void RaiseMatchEnded(int winnerId)

        => OnMatchEnded?.Invoke(winnerId);





    // =====================================================

    // CHARACTER SELECT

    // =====================================================



    public static event Action<int, int> OnCharacterSelected;

    // playerID, characterID

    public static void RaiseCharacterSelected(int playerID, int characterID)

        => OnCharacterSelected?.Invoke(playerID, characterID);



    public static event Action OnAllCharactersSelected;

    public static void RaiseAllCharactersSelected()

        => OnAllCharactersSelected?.Invoke();



    public static event Action OnCharacterSelectionStarted;

    public static void RaiseCharacterSelectionStarted()

        => OnCharacterSelectionStarted?.Invoke();





    // =====================================================

    // TIMER

    // =====================================================



    public static event Action<float> OnTimerTick; // time remaining

    public static void RaiseTimerTick(float timeRemaining)

        => OnTimerTick?.Invoke(timeRemaining);



    public static event Action OnTimeOut;

    public static void RaiseTimeOut()

        => OnTimeOut?.Invoke();

    public static event Action<int> OnRoundStarted; // round number
    public static void RaiseRoundStarted(int round) => OnRoundStarted?.Invoke(round);

    public static event Action<int> OnCoreSelectionStarted; // Thêm int để biết P1 hay P2 chọn
    public static void RaiseCoreSelectionStarted(int playerID) => OnCoreSelectionStarted?.Invoke(playerID);

    public static event Action OnCoreSelectionFinished;
    public static void RaiseCoreSelectionFinished() => OnCoreSelectionFinished?.Invoke();



    // =====================================================

    // PAUSE SYSTEM

    // =====================================================



    public static event Action OnGamePaused;

    public static void RaiseGamePaused()

        => OnGamePaused?.Invoke();



    public static event Action OnGameResumed;

    public static void RaiseGameResumed()

        => OnGameResumed?.Invoke();





    // =====================================================

    // COMBAT

    // =====================================================



    public static event Action<int> OnHitConfirmed; // attackerID

    public static void RaiseHitConfirmed(int attackerID)

        => OnHitConfirmed?.Invoke(attackerID);



    public static event Action<int, int> OnTakeDamage; // playerID, damage

    public static void RaiseTakeDamage(int playerID, int damage)

        => OnTakeDamage?.Invoke(playerID, damage);



    public static event Action<int> OnPlayerDied; // playerID

    public static void RaisePlayerDied(int playerID)

        => OnPlayerDied?.Invoke(playerID);





    // =====================================================

    // HEALTH

    // =====================================================



    public static event Action<int, int> OnHealthChanged;

    // playerID, currentHP

    public static void RaiseHealthChanged(int playerID, int currentHP)

        => OnHealthChanged?.Invoke(playerID, currentHP);





    // =====================================================

    // UI

    // =====================================================



    public static event Action<int> OnShowKO; // winnerID

    public static void RaiseShowKO(int winnerID)

        => OnShowKO?.Invoke(winnerID);



    public static event Action<int> OnShowWinScreen; // winnerID

    public static void RaiseShowWinScreen(int winnerID)

        => OnShowWinScreen?.Invoke(winnerID);

    // =====================================================
    // ENERGY
    // =====================================================

    public static event Action<int, int> OnManaChanged; // playerID, currentMana

    public static void RaiseManaChanged(int playerID, int currentMana)
        => OnManaChanged?.Invoke(playerID, currentMana);

}