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

        switch (gameState)
        {
            case E_GAMESTATE.Play:
                StatusImage.color = UIColor.Green;
                break;
            default:
                StatusImage.color = UIColor.Gray;
                break;
        }
        StatusText.text = gameState.ToString();
    }

    private void SetTimeText()
    {
        time = (int)GameManager.I.GetTime();
        timeText.text = (time / 60).ToString("D2") + ":" + (time % 60).ToString("D2");
    }
}
