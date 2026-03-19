using UnityEngine;

/// <summary>
/// Quản lý Pause/Resume game
/// Khi bấm ESC: Pause game, disable actions, set state thành Paused
/// Khi bấm ESC lại: Resume game, enable actions, set state thành Fighting
/// </summary>
public class PauseManager : MonoBehaviour
{
    private bool isPaused = false;
    private GameState stateBeforePause = GameState.Fighting;

    private void Update()
    {
        // Kiểm tra xem người chơi bấm ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                // ✅ CHỈ cho phép pause khi đang Fighting
                if (GameManager.Instance.currentState == GameState.Fighting)
                {
                    PauseGame();
                }
                else
                {
                    Debug.Log($"<color=orange>[Pause]</color> Cannot pause in {GameManager.Instance.currentState} state");
                }
            }
        }
    }

    /// <summary>
    /// Pause game: disable actions, set state thành Paused, phát event
    /// ✅ CHỈ hoạt động khi state = Fighting
    /// </summary>
    public void PauseGame()
    {
        // ✅ Guard: Chỉ pause nếu đang Fighting
        if (GameManager.Instance.currentState != GameState.Fighting)
        {
            Debug.LogWarning("<color=red>[Pause]</color> Cannot pause - not in Fighting state!");
            return;
        }

        if (isPaused) return; // Đã pause rồi

        isPaused = true;
        stateBeforePause = GameManager.Instance.currentState;

        // Khóa cử động của tất cả player
        GameManager.Instance.SetAllPlayersActionsPublic(false);

        // Set trạng thái game thành Paused
        GameManager.Instance.ChangeStatePublic(GameState.Paused);

        // Phát event Pause
        GameEvents.RaiseGamePaused();

        Debug.Log("<color=yellow>[Pause]</color> Game paused!");

        // Optional: Dừng thời gian để tạo hiệu ứng pause thực sự
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Resume game: enable actions, restore previous state, phát event
    /// </summary>
    public void ResumeGame()
    {
        if (!isPaused) return; // Chưa pause

        isPaused = false;

        // Mở cử động của tất cả player
        GameManager.Instance.SetAllPlayersActionsPublic(true);
        
        // Restore trạng thái game về trạng thái trước khi pause
        GameManager.Instance.ChangeStatePublic(stateBeforePause);
        
        // Phát event Resume
        GameEvents.RaiseGameResumed();

        Debug.Log("<color=cyan>[Resume]</color> Game resumed!");

        // Optional: Khôi phục thời gian bình thường
        // Time.timeScale = 1f;
    }

    public bool IsPaused => isPaused;
}
