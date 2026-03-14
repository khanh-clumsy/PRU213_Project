using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Trap : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 10;     // Lượng máu bị trừ
    public float damageCooldown = 1f;  // Thời gian chờ giữa 2 lần gây sát thương

    // Dictionary to track damage coroutines for each player
    private Dictionary<Collider2D, Coroutine> activeDamageCoroutines = 
        new Dictionary<Collider2D, Coroutine>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"[TRAP] Player entered trap at {Time.time:F2}s");

            // Start the damage coroutine for this specific player
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
    /// </summary>
    private IEnumerator DamagePlayerContinuously(Collider2D playerCollider)
    {
        // Apply damage immediately on first contact
        ApplyDamage(playerCollider.gameObject);

        // Wait for the cooldown duration
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

            // Raise the damage event (existing system)
            GameEvents.RaiseTakeDamage(id, damageAmount);

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