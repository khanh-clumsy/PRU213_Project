/// <summary>
/// Định nghĩa các trạng thái của game
/// </summary>
public enum GameState
{
    CharacterSelection,  // Chọn nhân vật
    Loading,             // Đang tải scene
    RoundStarting,       // Trạng thái đếm ngược mỗi hiệp
    Fighting,            // Đang đánh
    RoundOver,           // Hết 1 hiệp
    CoreSelection,       // Chọn lõi giữa các hiệp
    Paused,              // Pause game
    MatchOver            // Kết thúc cả trận đấu
}
