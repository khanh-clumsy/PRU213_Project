using UnityEngine;

public class CoreTest : MonoBehaviour
{
    void Update()
    {
        // Bấm T để mở bảng chọn lõi cho Player 1
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("[TEST] Mở bảng chọn lõi cho Player 1...");
            GameEvents.RaiseCoreSelectionStarted(1);
        }
    }
}