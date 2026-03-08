using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public int player1Character = -1;
    public int player2Character = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}