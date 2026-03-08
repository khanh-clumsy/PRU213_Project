using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLoader : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform player1SpawnPoint;
    public Transform player2SpawnPoint;

    private void Start()
    {
        if (GameData.Instance == null) return;

        int p1 = GameData.Instance.player1Character;
        int p2 = GameData.Instance.player2Character;

        if (p1 >= 0 && p1 < characterPrefabs.Length)
        {
            Instantiate(characterPrefabs[p1], player1SpawnPoint.position, Quaternion.identity);
        }

        if (p2 >= 0 && p2 < characterPrefabs.Length)
        {
            Instantiate(characterPrefabs[p2], player2SpawnPoint.position, Quaternion.identity);
        }
    }
}