using UnityEngine;

public class CoreTest : MonoBehaviour
{
    public CoreUIHandler handler;

    void Update()
    {
        // Bấm phím T (Test) để hiện bảng chọn lõi
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Đang test hiện bảng chọn lõi...");
            handler.ShowRandomCores();
        }
    }
}