using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 5;      // Lượng máu bị trừ
    public float damageCooldown = 2f;  // ✅ CHANGED: 2 giây giữa các lần gây sát thương

    // Dictionary to track damage coroutines for each player
    private Dictionary<Collider2D, Coroutine> activeDamageCoroutines = 
        new Dictionary<Collider2D, Coroutine>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[TRAP] Player entered trap at {Time.time:F2}s");

            // ✅ CHANGED: Stop old coroutine nếu tồn tại (nếu vừa ra vừa vào lại)
            if (activeDamageCoroutines.ContainsKey(other))
            {
                Debug.Log($"[TRAP] Old coroutine found, stopping it");
                StopCoroutine(activeDamageCoroutines[other]);
                activeDamageCoroutines.Remove(other);
            }

            // ✅ CHANGED: Damage ngay lập tức khi vào trap
            ApplyDamage(other.gameObject);

            // Start the damage coroutine for repeated damage every 2 seconds
            Coroutine damageCoroutine = StartCoroutine(DamagePlayerContinuously(other));
            activeDamageCoroutines[other] = damageCoroutine;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[TRAP] Player exited trap at {Time.time:F2}s");

            // Stop the damage coroutine for this player
            if (activeDamageCoroutines.ContainsKey(other))
            {
                StopCoroutine(activeDamageCoroutines[other]);
                activeDamageCoroutines.Remove(other);
                Debug.Log($"[TRAP] Stopped damage coroutine for player");
            }
        }
    }

    /// <summary>
    /// Coroutine that applies damage at fixed intervals while player stays in trigger
    /// ✅ CHANGED: Damage ngay lúc vào, sau đó 2 giây trừ 1 lần
    /// </summary>
    private IEnumerator DamagePlayerContinuously(Collider2D playerCollider)
    {
        // ✅ CHANGED: Chờ 2 giây TRƯỚC khi gây damagƯe tiếp theo
        // (damage đầu tiên đã được gây ở OnTriggerEnter2D)
        yield return new WaitForSeconds(damageCooldown);

        // Then continue applying damage at regular intervals
        while (activeDamageCoroutines.ContainsKey(playerCollider))
        {
            // Apply damage
            ApplyDamage(playerCollider.gameObject);

            // Wait for the cooldown duration before next damage
            yield return new WaitForSeconds(damageCooldown);
        }

        Debug.Log($"[TRAP] Damage coroutine ended for player");
    }

    private void ApplyDamage(GameObject playerObj)
    {
        var playerCtrl = playerObj.GetComponent<Player>();

        if (playerCtrl != null)
        {
            int id = playerCtrl.playerID;

            // ✅ CHANGED: Use RaiseTrapDamage instead of RaiseTakeDamage
            // This allows Player to differentiate trap damage from combat damage
            GameEvents.RaiseTrapDamage(id, damageAmount);

            Debug.Log($"[TRAP] Damage applied to Player {id}! " +
                      $"Damage: {damageAmount}. " +
                      $"Next damage in {damageCooldown}s");
        }
        else
        {
            Debug.LogWarning($"[TRAP] Could not find Player component on {playerObj.name}");
        }
    }

    /// <summary>
    /// Get the number of players currently in the trap
    /// </summary>
    public int GetPlayersInTrap() => activeDamageCoroutines.Count;

    /// <summary>
    /// Check if a specific player is currently in the trap
    /// </summary>
    public bool IsPlayerInTrap(Collider2D collider) => activeDamageCoroutines.ContainsKey(collider);
}