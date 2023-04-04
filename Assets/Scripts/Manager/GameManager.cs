using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System;

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

    /* 방 속성(유저(방장)가 설정 가능) */
    double timeForPrepare;   // 준비 시간
    double timeForPlay;      // 플레이 시간
    double bonusTimeForKill;      // 사람 한명 죽을때마다 추가시간
    double timeForCooling;   // 게임 끝나고 기다리는 시간

    int numberOfMafia;       // 마피아 수
    int numberOfDetective;   // 탐정 수
    /***************************/
    public Dictionary<int, E_PlayerRole> playerRoles = new Dictionary<int, E_PlayerRole>();    // 플레이어 역할 데이터, int = 플레이어의 Actor number

    public PhotonView PV;

    Hashtable ht_CustomValue;

    void Start()
    {
        gameState = E_GAMESTATE.Wait;
        timeForPrepare = 5f;
        timeForPlay = 20f;
        bonusTimeForKill = 30f;
        timeForCooling = 5f;
        numberOfMafia = 1;
        numberOfDetective = 0;

        playerRoles = new Dictionary<int, E_PlayerRole>();

        if (PhotonNetwork.IsMasterClient)
        {
            ht_CustomValue = new ExitGames.Client.Photon.Hashtable();
            startTime = PhotonNetwork.Time;
            ht_CustomValue.Add("StartTime", startTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(ht_CustomValue);
        }
        else    // 게스트에겐 이미 진행되고있는 시간을 표시
        {
            startTime = double.Parse(PhotonNetwork.CurrentRoom.CustomProperties["StartTime"].ToString());
        }
    }

    void Update()
    {
        //if(PhotonNetwork.IsMasterClient)
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
            //startTime = PhotonNetwork.Time;
            //endTime = timeForPrepare;
            //gameState = E_GAMESTATE.Prepare;
            if (PhotonNetwork.IsMasterClient)
                PV.RPC("SetGameStateRPC", RpcTarget.AllBuffered, PhotonNetwork.Time, timeForPrepare, E_GAMESTATE.Prepare);
        }
    }

    void UpdatePrepareProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForPrepare)
        {
            //foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
            //{
            //    Debug.Log(player.NickName);
            //}
            //startTime = PhotonNetwork.Time;
            //endTime = timeForPlay;
            //gameState = E_GAMESTATE.Play;
            if (PhotonNetwork.IsMasterClient)
            {
                SetPlayerRole();
                PV.RPC("SetGameStateRPC", RpcTarget.AllBuffered, PhotonNetwork.Time, timeForPlay, E_GAMESTATE.Play);
            }
            ////else
            //{
            //    //.Parse(PhotonNetwork.CurrentRoom.CustomProperties["PlayerRoles"].ToString());

            //    //playerRoles.Clear();
            //    //Hashtable ht = PhotonNetwork.CurrentRoom.CustomProperties;
            //    //ht.TryGetValue("PlayerRoles", out var pr);
            //    //playerRoles = (Dictionary<int, E_PlayerRole>)pr;
            //    //playerRoles = (Dictionary<int, E_PlayerRole>)PhotonNetwork.CurrentRoom.CustomProperties["PlayerRoles"];
            //}
        }
    }

    // 마스터 클라이언트가 모든 플레이어의 역할을 정해주고 나머지 클라이언트에게 알려준다
    void SetPlayerRole()
    {
        playerRoles.Clear();
        Player[] sortedPlayers = PhotonNetwork.PlayerList;

        for (int i = 0; i < sortedPlayers.Length; i ++)   // 전부 시민으로 초기화
        {
            playerRoles.Add(sortedPlayers[i].ActorNumber, E_PlayerRole.Civil);
        }

        for (int i = 0; i < numberOfMafia; i++)   // 마피아 뽑기
        {
            int index = UnityEngine.Random.Range(0, sortedPlayers.Length);
            if (GetPlayerRole(sortedPlayers[index].ActorNumber) != E_PlayerRole.Civil)  // 랜덤으로 뽑은 플레이어가 이미 마피아나 경찰이면 다시뽑음
            {
                i--;
                continue;
            }
            playerRoles[sortedPlayers[index].ActorNumber] = E_PlayerRole.Mafia;

        }

        for (int i = 0; i < numberOfDetective; i++)   // 탐정 뽑기
        {
            int index = UnityEngine.Random.Range(0, sortedPlayers.Length);
            if (GetPlayerRole(sortedPlayers[index].ActorNumber) != E_PlayerRole.Civil)  // 랜덤으로 뽑은 플레이어가 이미 마피아나 경찰이면 다시뽑음
            {
                i--;
                continue;
            }
            playerRoles[sortedPlayers[index].ActorNumber] = E_PlayerRole.Detective;

        }

        // RPC는 dictionary를 받지 못하므로, playerRoles dictionary를 string으로 변환하여 파라미터로 준다.
        string playerRolesString = StringConverter.I.ConvertDictionaryToString<int, E_PlayerRole>(playerRoles);
        PV.RPC("SetPlayerRoleRPC", RpcTarget.AllBuffered, playerRolesString);

        for (int i = 0; i < sortedPlayers.Length; i ++)
        {
            Debug.Log(sortedPlayers[i].NickName + " : " + playerRoles[sortedPlayers[i].ActorNumber].ToString());
        }

    }

    // playerRoles를 string으로 받아 int, E_PlayerRole형 dictionary로 변환하여 모든 클라이언트에게 전해준다.
    [PunRPC]
    void SetPlayerRoleRPC(string playerRolesString)
    {
        Dictionary<string, string> playerRolesStringDic = StringConverter.I.ConvertStringToDictionary(playerRolesString);

        playerRoles.Clear();
        foreach (KeyValuePair<string, string> kvPair in playerRolesStringDic)
        {
            playerRoles.Add(int.Parse(kvPair.Key), (E_PlayerRole)Enum.Parse(typeof(E_PlayerRole), kvPair.Value));
        }


    }

    public E_PlayerRole GetPlayerRole(int actorNumber)  // 특정 유저의 역할 받기
    {
        playerRoles.TryGetValue(actorNumber, out E_PlayerRole roleToGet);

        return roleToGet;
    }
    public E_PlayerRole GetPlayerRole() // 자기자신의 역할 받기
    {
        playerRoles.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out E_PlayerRole roleToGet);

        return roleToGet;
    }

    void UpdatePlayProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForPlay)
        {
            //startTime = PhotonNetwork.Time;
            //endTime = timeForCooling;
            //gameState = E_GAMESTATE.Cooling;
            if (PhotonNetwork.IsMasterClient)
                PV.RPC("SetGameStateRPC", RpcTarget.AllBuffered, PhotonNetwork.Time, timeForCooling, E_GAMESTATE.Cooling);
        }
    }

    void UpdateCoolingProcess()
    {
        timer = PhotonNetwork.Time - startTime;

        if (timer >= timeForCooling)
        {
            //gameState = E_GAMESTATE.Wait;
            if (PhotonNetwork.IsMasterClient)
                PV.RPC("SetGameStateRPC", RpcTarget.AllBuffered, PhotonNetwork.Time, timeForCooling, E_GAMESTATE.Wait);
        }
    }

    [PunRPC]
    void SetGameStateRPC(double _startTime, double _endTime, E_GAMESTATE _gameState)
    {
        startTime = _startTime;
        endTime = _endTime;
        gameState = _gameState;
    }

    public double GetTime()
    {
        return endTime - timer;
    }
}
