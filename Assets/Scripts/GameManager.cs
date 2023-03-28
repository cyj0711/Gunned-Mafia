using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum E_GAMESTATE  // 현재 게임 방의 진행 상태
{
    Wait,       // 둘 이상의 유저를 기다리는 상태 (1명일때만 이 상태 유지)
    Prepare,    // 충분한 수의 유저(둘 이상)가 모여, 게임 시작을 기다리는 상태(카운트다운)
    Play,       // 게임이 시작되고 진행중인 상태(카운트다운)
    Cooling     // 모든 게임이 끝나고 새 라운드를 기다리는 상태(카운트다운)
}

public class GameManager : SingletonPunCallbacks<GameManager>
{
    private E_GAMESTATE gameState;
    public E_GAMESTATE GameState { get { return gameState; } }

    double timer;

    double startTime;
    double endTime;

    double timeForPrepare;   // 준비 시간
    double timeForPlay;      // 플레이 시간
    double bonusTimeForKill;      // 사람 한명 죽을때마다 추가시간
    double timeForCooling;   // 게임 끝나고 기다리는 시간

    Hashtable CustomValue;

    void Start()
    {
        gameState = E_GAMESTATE.Wait;
        timeForPrepare = 20f;
        timeForPlay = 300f;
        bonusTimeForKill = 30f;
        timeForCooling = 20f;

        if (PhotonNetwork.IsMasterClient)
        {
            CustomValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            CustomValue.Add("StartTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(CustomValue);
        }
        else
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        }
    }

    void Update()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            switch(gameState)
            {
                case E_GAMESTATE.Wait:
                    UpdateWaitProcess();
                    break;
                case E_GAMESTATE.Prepare:
                    UpdatePrepareProcess();
                    break;
                case E_GAMESTATE.Play:
                    UpdatePlayProcess();
                    break;
                case E_GAMESTATE.Cooling:
                    UpdateCoolingProcess();
                    break;
            }
        }
        // Debug.Log(gameState + " " + timer);
    }

    void UpdateWaitProcess()
    {
        if(PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            startTime = PhotonNetwork.Time;
            endTime = timeForPrepare;
            gameState = E_GAMESTATE.Prepare;
        }
    }

    void UpdatePrepareProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForPrepare)
        {
            startTime = PhotonNetwork.Time;
            endTime = timeForPlay;
            gameState = E_GAMESTATE.Play;
        }
    }

    void UpdatePlayProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForPlay)
        {
            startTime = PhotonNetwork.Time;
            endTime = timeForCooling;
            gameState = E_GAMESTATE.Cooling;
        }
    }

    void UpdateCoolingProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForCooling)
        {
            gameState = E_GAMESTATE.Wait;
        }
    }

    public double GetTime()
    {
        return endTime - timer;
    }
}
