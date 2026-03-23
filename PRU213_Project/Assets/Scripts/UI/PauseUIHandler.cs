using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Quản lý UI Pause Panel
/// Hiển thị/ẩn pause menu khi bấm ESC
/// Cung cấp nút Resume và Settings
/// </summary>
public class PauseUIHandler : MonoBehaviour
{
    [Header("Pause Panel")]
    public GameObject pausePanel; // Root panel chứa toàn bộ pause menu

    [Header("Buttons")]
    public Button resumeButton;
    public Button settingsButton;
    public Button exitButton;

    [Header("Panels")]
    public GameObject pauseTitlePanel; // Panel hiển thị "PAUSED"

    private PauseManager pauseManager;

    private void Start()
    {
        // Tìm PauseManager trong scene
        pauseManager = FindObjectOfType<PauseManager>();

        if (pauseManager == null)
        {
            Debug.LogError("PauseUIHandler không tìm thấy PauseManager!");
            return;
        }

        // Setup button listeners
        if (resumeButton != null)
            resumeButton.onClick.AddListener(() => pauseManager.ResumeGame());

        if (exitButton != null)
            exitButton.onClick.AddListener(() => ExitGame());

        if (settingsButton != null)
            settingsButton.onClick.AddListener(() => Debug.Log("Settings clicked"));

        // Subscribe tới pause/resume events
        GameEvents.OnGamePaused += ShowPauseMenu;
        GameEvents.OnGameResumed += HidePauseMenu;

        // Ẩn pause menu lúc khởi động
        HidePauseMenu();
    }

    private void OnDestroy()
    {
        // Unsubscribe events
        GameEvents.OnGamePaused -= ShowPauseMenu;
        GameEvents.OnGameResumed -= HidePauseMenu;

        // Unregister buttons
        if (resumeButton != null)
            resumeButton.onClick.RemoveAllListeners();
        if (exitButton != null)
            exitButton.onClick.RemoveAllListeners();
        if (settingsButton != null)
            settingsButton.onClick.RemoveAllListeners();
    }

    /// <summary>
    /// Hiển thị Pause Menu khi game bị pause
    /// ✅ CHỈ hiển thị nếu state = Paused
    /// </summary>
    private void ShowPauseMenu()
    {
        // ✅ Guard: Chỉ show menu khi state Paused
        if (GameManager.Instance.currentState != GameState.Paused)
        {
            Debug.LogWarning("<color=red>[PauseUI]</color> Pause menu called but state is not Paused!");
            return;
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Debug.Log("<color=yellow>[UI]</color> Pause menu displayed");
        }
    }

    /// <summary>
    /// Ẩn Pause Menu khi game resume
    /// </summary>
    private void HidePauseMenu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Debug.Log("<color=cyan>[UI]</color> Pause menu hidden");
        }
    }

    private void ExitGame()
    {
        Debug.Log("<color=red>[UI]</color> Going back to start menu...");
        Time.timeScale = 1f;  // Ensure time is running before scene change
        SceneManager.LoadScene("StartScene");
    }
}
