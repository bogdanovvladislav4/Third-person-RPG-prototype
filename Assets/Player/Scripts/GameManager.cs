using System;
using System.Collections;
using System.Collections.Generic;
using LateExe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public enum GameStage
    {
        StartGame,
        Play, 
        CombatMode,
        GameOver,
        MainMenu,
        Inventory
    }

    public GameStage gameStage;
    public GameObject meleWeapons;

    [SerializeField] private Player player;

    private void Update()
    {
        PlayerDeathCheck();
    }

    void PlayerDeathCheck()
    {
        if (player.playerState == PlayerStates.Death)
        {
            Invoke(nameof(ReloadScene), 5);
        }
    }

    void ReloadScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}
