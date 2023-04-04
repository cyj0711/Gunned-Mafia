using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanelManager : Singleton<GamePanelManager>
{
    public Text timeText;
    public Text StatusText;
    public Image StatusImage;

    int time;
    E_GAMESTATE gameState;
    E_PlayerRole playerRole;
    
    void Start()
    {
        SetGameState();
        SetTimeText();
    }

    void Update()
    {
        SetGameState();
        SetTimeText();
    }

    private void SetGameState()
    {
        gameState = GameManager.I.GameState;
        playerRole = GameManager.I.GetPlayerRole();

        switch (gameState)
        {
            case E_GAMESTATE.Play:
                SetPlayingState();
                break;
            default:
                StatusImage.color = UIColor.Gray;
                StatusText.text = gameState.ToString();
                break;
        }

    }

    private void SetPlayingState()
    {
        switch(playerRole)
        {
            case E_PlayerRole.Civil:
                StatusImage.color = UIColor.Green;
                break;
            case E_PlayerRole.Mafia:
                StatusImage.color = UIColor.Red;
                break;
            case E_PlayerRole.Detective:
                StatusImage.color = UIColor.Blue;
                break;
            default:
                StatusImage.color = UIColor.Gray;
                break;
        }
        StatusText.text = playerRole.ToString();
    }

    private void SetTimeText()
    {
        time = (int)GameManager.I.GetTime();
        timeText.text = (time / 60).ToString("D2") + ":" + (time % 60).ToString("D2");
    }
}
