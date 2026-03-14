using UnityEngine;

public class UITestInput : MonoBehaviour
{
    private int p1HP = 100;
    private int p2HP = 100;
    private int p1Energy = 0;
    private int p2Energy = 0;

    private void Start()
    {
        GameEvents.RaiseHealthChanged(1, p1HP);
        GameEvents.RaiseHealthChanged(2, p2HP);
        GameEvents.RaiseManaChanged(1, p1Energy);
        GameEvents.RaiseManaChanged(2, p2Energy);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            p1HP -= 10;
            if (p1HP < 0) p1HP = 0;
            GameEvents.RaiseHealthChanged(1, p1HP);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            p2HP -= 10;
            if (p2HP < 0) p2HP = 0;
            GameEvents.RaiseHealthChanged(2, p2HP);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            p1Energy += 10;
            if (p1Energy > 100) p1Energy = 100;
            GameEvents.RaiseManaChanged(1, p1Energy);
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            p2Energy += 10;
            if (p2Energy > 100) p2Energy = 100;
            GameEvents.RaiseManaChanged(2, p2Energy);
        }
    }
}