using UnityEngine;

public class Health : MonoBehaviour
{
    public int playerID; // 1 hoặc 2
    public int maxHealth = 100;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        // Báo cho UI và GameManager biết máu lúc bắt đầu
        GameEvents.RaiseHealthChanged(playerID, currentHealth);
    }

    private void OnEnable()
    {
        // Nghe Event sát thương toàn cục, nếu ID trùng với mình thì trừ máu
        GameEvents.OnTakeDamage += TakeDamage;
    }

    public void TakeDamage(int targetID, int damage)
    {
        if (targetID != playerID) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Cập nhật cho GameManager và UI
        GameEvents.RaiseHealthChanged(playerID, currentHealth);

        if (currentHealth <= 0)
        {
            GameEvents.RaisePlayerDied(playerID);
        }
    }
}