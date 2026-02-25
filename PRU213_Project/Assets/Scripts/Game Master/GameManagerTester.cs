using UnityEngine;

public class GameManagerTester : MonoBehaviour
{
    private void OnEnable()
    {
        // Đăng ký nghe thử xem GameManager có bắn event đếm ngược không
        GameEvents.OnGameStateChanged += LogState;
        GameEvents.OnCountdownTick += LogCountdown;
        GameEvents.OnMatchStarted += LogMatchStart;
        GameEvents.OnTimerTick += LogTimer;
    }

    private void OnDisable()
    {
        GameEvents.OnGameStateChanged -= LogState;
        GameEvents.OnCountdownTick -= LogCountdown;
        GameEvents.OnMatchStarted -= LogMatchStart;
        GameEvents.OnTimerTick -= LogTimer;
    }

    private void Update()
    {
        // 1. Bấm phím Space để GIẢ LẬP việc 2 người đã chọn tướng xong
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("TESTER: Đã bấm Space. Giả lập gọi Event AllCharactersSelected...");
            GameEvents.RaiseAllCharactersSelected();
        }

        // 2. Bấm phím K để GIẢ LẬP Player 1 bị chết
        if (Input.GetKeyDown(KeyCode.K))
        {
            Debug.Log("TESTER: Bấm K. Giả lập Player 1 chết...");
            GameEvents.RaisePlayerDied(1); // 1 là ID của Player 1
        }
    }

    // Các hàm in ra Console để kiểm tra
    private void LogState(GameState state) => Debug.Log("TRẠNG THÁI GAME ĐỔI THÀNH: " + state);
    private void LogCountdown(int count) => Debug.Log("ĐẾM NGƯỢC: " + count);
    private void LogMatchStart() => Debug.Log("FIGHT!!! TRẬN ĐẤU BẮT ĐẦU!");

    // In số giây ra (dùng Mathf.RoundToInt để Console đỡ bị spam số thập phân)
    private void LogTimer(float time) => Debug.Log("Thời gian còn: " + Mathf.RoundToInt(time) + "s");
}