﻿using System.Collections;
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

public class GameManager : Singleton<GameManager>
{
    private E_GAMESTATE m_eGameState;
    public E_GAMESTATE a_eGameState { get { return m_eGameState; } }

    double m_dProcessTimer;
    double m_dStartTime;
    double m_dEndTime;

    int m_iCurrentNumberOfMafia;
    int m_iCurrentNumberOfCivil;

    private Dictionary<int, PlayerController> m_dicPlayerController = new Dictionary<int, PlayerController>();
    private Dictionary<int, string> m_dicPlayerNickName = new Dictionary<int, string>();

    /* 방 속성(유저(방장)가 설정 가능) */
    double m_dPropertyTimeForPrepare;   // 준비 시간
    double m_dPropertyTimeForPlay;      // 플레이 시간
    double m_dPropertyBonusTimeForKill;      // 사람 한명 죽을때마다 추가시간
    double m_dPropertyTimeForCooling;   // 게임 끝나고 기다리는 시간

    int m_iPropertyNumberOfMafia;       // 마피아 수
    int m_iPropertyNumberOfDetective;   // 탐정 수
    /***************************/
    private Dictionary<int, E_PlayerRole> m_dicPlayerRoles = new Dictionary<int, E_PlayerRole>();    // 플레이어 역할 데이터, int = 플레이어의 Actor number
    //private Dictionary<int, E_PlayerRole> m_dicLivingPlayerRoles; // 현재 살아있는 플레이어 역할 데이터

    public PhotonView m_vPhotonView;

    Hashtable m_htCustomValue;

    private bool m_bIsRoleSet;

    void Start()
    {
        m_eGameState = E_GAMESTATE.Wait;
        m_dPropertyTimeForPrepare = 5f;
        m_dPropertyTimeForPlay = 300f;
        m_dPropertyBonusTimeForKill = 30f;
        m_dPropertyTimeForCooling = 5f;
        m_iPropertyNumberOfMafia = 2;
        m_iPropertyNumberOfDetective = 1;

        m_dicPlayerRoles = new Dictionary<int, E_PlayerRole>();

        if (PhotonNetwork.IsMasterClient)
        {
            //m_dStartTime = PhotonNetwork.Time;
            m_htCustomValue = new Hashtable();

            m_htCustomValue.Add("GameState", m_eGameState);
            m_htCustomValue.Add("StartTime", PhotonNetwork.Time);
            m_htCustomValue.Add("EndTime", -1.0);

            //m_htCustomValue.Add("StartTime", m_dStartTime);
            PhotonNetwork.CurrentRoom.SetCustomProperties(m_htCustomValue);
        }
        else    // 게스트에겐 이미 진행되고있는 시간을 표시
        {
            m_htCustomValue = PhotonNetwork.CurrentRoom.CustomProperties;
            m_eGameState = (E_GAMESTATE)((int)m_htCustomValue["GameState"]);
            m_dStartTime = (double)m_htCustomValue["StartTime"];
            m_dEndTime = (double)m_htCustomValue["EndTime"];
        }
        UIGameManager.I.SetGameState();
    }

    void Update()
    {
        //if(PhotonNetwork.IsMasterClient)
        {
            switch(m_eGameState)
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

    // 게임을 재시작하기전에 기존 변수들 초기화
    private void InitVariable()
    {
        m_bIsRoleSet = false;
        MapManager.I.InitVariable();
        UIGameManager.I.InitVariable();
        UISearchManager.I.SetSearchPanelActive(false);
    }

    public void AddPlayerController(int _iActorNumber, PlayerController _vPlayerController)
    {
        if (m_dicPlayerController.ContainsKey(_iActorNumber))
        {
            m_dicPlayerController[_iActorNumber] = _vPlayerController;
        }
        else
        {
            m_dicPlayerController.Add(_iActorNumber, _vPlayerController);
        }

        AddPlayerNickName(_iActorNumber, _vPlayerController.a_vPhotonView.Owner.NickName);

    }

    public void RemovePlayerController(int _iActorNumber)
    {
        if (m_dicPlayerController.ContainsKey(_iActorNumber))
        {
            m_dicPlayerController.Remove(_iActorNumber);
        }

    }

    // ActorNumber에 해당하는 PlayerController 반환
    public PlayerController GetPlayerController(int _iActorNumber)
    {
        if (m_dicPlayerController.ContainsKey(_iActorNumber))
            return m_dicPlayerController[_iActorNumber];

        return null;
    }

    // 로컬 플레이어의 PlayerController 반환
    public PlayerController GetPlayerController()
    {
        int _iActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;

        if (m_dicPlayerController.ContainsKey(_iActorNumber))
            return m_dicPlayerController[_iActorNumber];

        return null;
    }

    public void AddPlayerNickName(int _iActorNumber, string _strNickName)
    {
        if (m_dicPlayerNickName.ContainsKey(_iActorNumber))
        {
            m_dicPlayerNickName[_iActorNumber] = _strNickName;
        }
        else
        {
            m_dicPlayerNickName.Add(_iActorNumber, _strNickName);
        }
    }

    // 나간 플레이어의 닉네임을 받기 위해 이때까지 참여한 모든 유저의 닉네임을 저장해둔다.
    public string GetPlayerNickName(int _iActorNumber)
    {
        if (m_dicPlayerNickName.ContainsKey(_iActorNumber))
            return m_dicPlayerNickName[_iActorNumber];

        return null;
    }

    // 플레이어가 사망하면 나머지 플레이어들의 진짜 직업(이름표 색깔)을 표시한다.
    public void PlayerNameColorUpdate()
    {
        foreach (KeyValuePair<int, PlayerController> _kvPair in m_dicPlayerController)
        {
            if(_kvPair.Value!=null)
                _kvPair.Value.a_vCharacterUIController.SetRoleUI();
        }
    }

    public void DisplayGhosts()
    {
        foreach (KeyValuePair<int, PlayerController> _kvPair in m_dicPlayerController)
        {
            if (_kvPair.Value != null)
            {
                _kvPair.Value.SetCharacterSprite(true);
                _kvPair.Value.a_vCharacterUIController.SetCanvasBodyActive(true);
            }
        }
    }

    void UpdateWaitProcess()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount >= (m_iPropertyNumberOfMafia + m_iPropertyNumberOfDetective))
        {
            //startTime = PhotonNetwork.Time;
            //endTime = timeForPrepare;
            //gameState = E_GAMESTATE.Prepare;
            if (PhotonNetwork.IsMasterClient)
            {
                //m_vPhotonView.RPC(nameof(SetGameStateRPC), RpcTarget.AllBuffered, PhotonNetwork.Time, m_dPropertyTimeForPrepare, E_GAMESTATE.Prepare);
                MapManager.I.SpawnWeapons();
                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForPrepare, E_GAMESTATE.Prepare);
            }
        }
    }

    void UpdatePrepareProcess()
    {
        m_dProcessTimer = PhotonNetwork.Time - m_dStartTime;

        if (m_dProcessTimer >= m_dPropertyTimeForPrepare)
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
                //m_vPhotonView.RPC(nameof(SetGameStateRPC), RpcTarget.AllBuffered, PhotonNetwork.Time, m_dPropertyTimeForPlay, E_GAMESTATE.Play);

                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForPlay, E_GAMESTATE.Play);
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
        if (m_bIsRoleSet)   // 서버 시간차이로 인한 SetPlayerRole 중복 호출을 방지한다.
            return;

        m_dicPlayerRoles.Clear();
        Player[] vSortedPlayers = PhotonNetwork.PlayerList;

        for (int i = 0; i < vSortedPlayers.Length; i ++)   // 전부 시민으로 초기화
        {
            m_dicPlayerRoles.Add(vSortedPlayers[i].ActorNumber, E_PlayerRole.Civil);
        }

        for (int i = 0; i < m_iPropertyNumberOfMafia; i++)   // 마피아 뽑기
        {
            int index = UnityEngine.Random.Range(0, vSortedPlayers.Length);
            if (GetPlayerRole(vSortedPlayers[index].ActorNumber) != E_PlayerRole.Civil)  // 랜덤으로 뽑은 플레이어가 이미 마피아나 경찰이면 다시뽑음
            {
                i--;
                continue;
            }
            m_dicPlayerRoles[vSortedPlayers[index].ActorNumber] = E_PlayerRole.Mafia;

        }

        for (int i = 0; i < m_iPropertyNumberOfDetective; i++)   // 탐정 뽑기
        {
            int index = UnityEngine.Random.Range(0, vSortedPlayers.Length);
            if (GetPlayerRole(vSortedPlayers[index].ActorNumber) != E_PlayerRole.Civil)  // 랜덤으로 뽑은 플레이어가 이미 마피아나 경찰이면 다시뽑음
            {
                i--;
                continue;
            }
            m_dicPlayerRoles[vSortedPlayers[index].ActorNumber] = E_PlayerRole.Detective;

        }

        // RPC는 dictionary를 받지 못하므로, playerRoles dictionary를 string으로 변환하여 파라미터로 준다.
        string strPlayerRoles = StringConverter.I.ConvertDictionaryToString<int, E_PlayerRole>(m_dicPlayerRoles);
        m_vPhotonView.RPC(nameof(SetPlayerRoleRPC), RpcTarget.All, strPlayerRoles, vSortedPlayers.Length - m_iPropertyNumberOfMafia, m_iPropertyNumberOfMafia);

        //for (int i = 0; i < vSortedPlayers.Length; i ++)
        //{
        //    Debug.Log(vSortedPlayers[i].NickName + " : " + m_dicPlayerRoles[vSortedPlayers[i].ActorNumber].ToString());
        //}

        m_bIsRoleSet = true;
    }

    // playerRoles를 string으로 받아 int, E_PlayerRole형 dictionary로 변환하여 모든 클라이언트에게 전해준다.
    [PunRPC]
    void SetPlayerRoleRPC(string _strPlayerRoles, int _iCurrentNumberOfCivil, int _iCurrentNumberOfMafia)
    {
        Dictionary<string, string> dicPlayerRolesString = StringConverter.I.ConvertStringToDictionary(_strPlayerRoles);

        m_dicPlayerRoles.Clear();
        foreach (KeyValuePair<string, string> kvPair in dicPlayerRolesString)
        {
            int iActorNumber = int.Parse(kvPair.Key);
            E_PlayerRole ePlayerRole = (E_PlayerRole)Enum.Parse(typeof(E_PlayerRole), kvPair.Value);
            //m_dicPlayerRoles.Add(int.Parse(kvPair.Key), (E_PlayerRole)Enum.Parse(typeof(E_PlayerRole), kvPair.Value));
            m_dicPlayerRoles.Add(iActorNumber, ePlayerRole);
        }

        m_iCurrentNumberOfCivil = _iCurrentNumberOfCivil;
        m_iCurrentNumberOfMafia = _iCurrentNumberOfMafia;

        int iLocalPlayerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        PlayerController vPlayerController = GetPlayerController(iLocalPlayerActorNumber);
        if (vPlayerController != null)
        {
            vPlayerController.a_ePlayerRole = m_dicPlayerRoles[iLocalPlayerActorNumber];
        }

    }

    public E_PlayerRole GetPlayerRole(int iActorNumber)  // 특정 유저의 역할 받기
    {
        m_dicPlayerRoles.TryGetValue(iActorNumber, out E_PlayerRole eRoleToGet);

        return eRoleToGet;
    }

    public E_PlayerRole GetPlayerRole() // 자기자신의 역할 받기
    {
        m_dicPlayerRoles.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out E_PlayerRole roleToGet);

        return roleToGet;
    }

    public List<int> GetDetectivePlayers()  // 탐정 역할을 가진 플레이어 받기
    {
        List<int> listDetectivePlayers = new List<int>();

        foreach(KeyValuePair<int, E_PlayerRole> kvPair in m_dicPlayerRoles)
        {
            if (kvPair.Value == E_PlayerRole.Detective)
                listDetectivePlayers.Add(kvPair.Key);
        }

        return listDetectivePlayers;
    }

    void UpdatePlayProcess()
    {
        m_dProcessTimer = PhotonNetwork.Time - m_dStartTime;

        if (m_dProcessTimer >= m_dPropertyTimeForPlay)
        {
            //startTime = PhotonNetwork.Time;
            //endTime = timeForCooling;
            //gameState = E_GAMESTATE.Cooling;
            if (PhotonNetwork.IsMasterClient)
            {
                //m_vPhotonView.RPC(nameof(SetGameStateRPC), RpcTarget.AllBuffered, PhotonNetwork.Time, m_dPropertyTimeForCooling, E_GAMESTATE.Cooling);

                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForCooling, E_GAMESTATE.Cooling);
            }
        }
    }

    // 플레이어가 죽거나 나갔을때 게임이 끝나는지 확인
    public void CheckGameOver(int _iPlayerActorNumber)
    {
        if (m_eGameState == E_GAMESTATE.Play)
        {
            m_vPhotonView.RPC(nameof(CheckGameOverRPC), RpcTarget.AllViaServer, _iPlayerActorNumber);
        }
        // 게임 준비 도중 인원수가 적어지면 다시 대기상태로 변경
        else if(m_eGameState==E_GAMESTATE.Prepare)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                if(PhotonNetwork.CurrentRoom.PlayerCount< m_iPropertyNumberOfMafia + m_iPropertyNumberOfDetective)
                    SetGameState(PhotonNetwork.Time, m_dPropertyTimeForPrepare, E_GAMESTATE.Prepare);
            }
        }
    }

    [PunRPC]
    private void CheckGameOverRPC(int _iPlayerActorNumber)
    {
       // m_dicLivingPlayerRoles.Remove(_iPlayerActorNumber);

        E_PlayerRole eDeadPlayerRole = GetPlayerRole(_iPlayerActorNumber);

        if (eDeadPlayerRole == E_PlayerRole.Mafia)
        {
            m_iCurrentNumberOfMafia -= 1;

            if (m_iCurrentNumberOfMafia <= 0 && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                UIGameManager.I.SendNotificationToAll("Civil Team Win !!");

                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForCooling, E_GAMESTATE.Cooling);
            }
        }
        else
        {
            m_iCurrentNumberOfCivil -= 1;
            if (m_iCurrentNumberOfCivil <= 0 && PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                UIGameManager.I.SendNotificationToAll("Mafia Team Win !!");

                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForCooling, E_GAMESTATE.Cooling);
            }
        }
    }

    void UpdateCoolingProcess()
    {
        m_dProcessTimer = PhotonNetwork.Time - m_dStartTime;

        if (m_dProcessTimer >= m_dPropertyTimeForCooling)
        {
            MapManager.I.RemoveAllWeapons();
            MapManager.I.RemoveAllBodies();
            GetPlayerController().a_vWeaponController.InitWeaponController();
            UISearchManager.I.RemoveDicPlayerLocationPingAll();
            ChatManager.I.ToggleTeamChat(false);

            InitVariable();

            if (PhotonNetwork.IsMasterClient)
            {
                RespawnAllPlayers();
                SetGameState(PhotonNetwork.Time, m_dPropertyTimeForCooling, E_GAMESTATE.Wait);
            }
        }
    }

    private void RespawnAllPlayers()
    {
        foreach (KeyValuePair<int, PlayerController> _kvPair in m_dicPlayerController)
        {
            // _kvPair.Value.transform.position = new Vector3(UnityEngine.Random.Range(-0.5f, 1f), UnityEngine.Random.Range(-1f, 0f));
            _kvPair.Value.TeleportPlayer(UnityEngine.Random.Range(-0.5f, 1f), UnityEngine.Random.Range(-1f, 0f));
            _kvPair.Value.a_ePlayerState = E_PlayerState.Alive;
            _kvPair.Value.a_ePlayerRole = E_PlayerRole.None;
            _kvPair.Value.a_iCurrentHealth = 100;
        }
    }

    void SetGameState(double _dStartTime, double _dEndTime, E_GAMESTATE _eGameState)
    {
        m_htCustomValue["GameState"] = (int)_eGameState;
        m_htCustomValue["StartTime"] = _dStartTime;
        m_htCustomValue["EndTime"] =  _dEndTime;

        PhotonNetwork.CurrentRoom.SetCustomProperties(m_htCustomValue);

        m_vPhotonView.RPC(nameof(SetGameStateRPC), RpcTarget.AllViaServer);
    }

    [PunRPC]
    void SetGameStateRPC()
    {
        m_htCustomValue = PhotonNetwork.CurrentRoom.CustomProperties;
        m_eGameState = (E_GAMESTATE)((int)m_htCustomValue["GameState"]);
        m_dStartTime = (double)m_htCustomValue["StartTime"];
        m_dEndTime = (double)m_htCustomValue["EndTime"];

        UIGameManager.I.SetGameState();
    }

    public double GetTime()
    {
        return m_dEndTime - m_dProcessTimer;
    }

    // 플레이어가 무기에 닿으면 서버를 통해 해당 무기를 얻을 수 있는지 확인받는다.
    public void CheckCanPlayerPickUpWeapon(int _iWeaponViewID, int _iPlayerActorNumber)
    {
        m_vPhotonView.RPC(nameof(CheckCanPlayerPickUpWeaponRPC), RpcTarget.MasterClient, _iWeaponViewID, _iPlayerActorNumber);
    }

    // 하나의 무기를 여러 플레이어가 동시에 주울때 꼬이는걸 막기위해 서버가 한명에게만 무기를 줍도록 조절한다.
    [PunRPC]
    private void CheckCanPlayerPickUpWeaponRPC(int _iWeaponViewID, int _iPlayerActorNumber)
    {
        if(!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.LogError(nameof(CheckCanPlayerPickUpWeaponRPC) + "must be called on the master client, BUT "+ PhotonNetwork.LocalPlayer.NickName+" is not MASTER!!");
            return;
        }

        WeaponBase vWeaponBase = PhotonView.Find(_iWeaponViewID).gameObject.GetComponent<WeaponBase>();

        if (vWeaponBase == null)
        {
            Debug.LogError("Player(" + _iPlayerActorNumber + ") tried to get weapon(" + _iWeaponViewID + "), But the weaponBase is null");
            return;
        }

        if (vWeaponBase.a_iOwnerPlayerActorNumber == -1)
        {
            vWeaponBase.a_iOwnerPlayerActorNumber = _iPlayerActorNumber;
            m_vPhotonView.RPC(nameof(ReturnCanPlayerPickUpWeaponRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iPlayerActorNumber), _iWeaponViewID, _iPlayerActorNumber, true);
        }
        else
        {
            m_vPhotonView.RPC(nameof(ReturnCanPlayerPickUpWeaponRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iPlayerActorNumber), _iWeaponViewID, _iPlayerActorNumber, false);
        }

    }

    // 무기를 주우려는 플레이어에게 무기 획득 가능 여부를 알려준다.
    [PunRPC]
    private void ReturnCanPlayerPickUpWeaponRPC(int _iWeaponViewID, int _iPlayerActorNumber, bool _bCanPickUp)
    {
        if(!_bCanPickUp)
        {
            Debug.LogWarning("Player(" + PhotonNetwork.LocalPlayer.NickName + ") tried to get weapon(" + _iWeaponViewID + "), But the weaponBase is not on the field");
            return;
        }

        WeaponController vWeaponController = GetPlayerController(_iPlayerActorNumber).a_vWeaponController;

        if (vWeaponController == null)
        {
            Debug.LogError("Player(" + PhotonNetwork.LocalPlayer.NickName + ") tried to get weapon(" + _iWeaponViewID + "), But the weaponManager is null");
            return;
        }

        vWeaponController.PickUpWeapon(_iWeaponViewID);

    }

    // 해당 플레이어가 시체의 첫번째 발견자인지 확인
    public void CheckIsPlayerFirstWitness(int _iBodyActorNumber, int _iPlayerActorNumber)
    {
        m_vPhotonView.RPC(nameof(CheckIsPlayerFirstWitnessRPC), RpcTarget.MasterClient, _iBodyActorNumber, _iPlayerActorNumber);
    }

    // 하나의 무기를 여러 플레이어가 동시에 주울때 꼬이는걸 막기위해 서버가 한명에게만 무기를 줍도록 조절한다.
    [PunRPC]
    private void CheckIsPlayerFirstWitnessRPC(int _iBodyActorNumber, int _iPlayerActorNumber)
    {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            Debug.LogError(nameof(CheckCanPlayerPickUpWeaponRPC) + "must be called on the master client, BUT " + PhotonNetwork.LocalPlayer.NickName + " is not MASTER!!");
            return;
        }

        PlayerDeadController vPlayerDeadController = MapManager.I.GetPlayerDead(_iBodyActorNumber);

        if (vPlayerDeadController == null)
        {
            Debug.LogError("Player(" + _iPlayerActorNumber + ") tried to search body (" + _iBodyActorNumber + "), But the body data is null");
            return;
        }

        if (vPlayerDeadController.a_iFirstWitnessActorNumber == -1)
        {
            vPlayerDeadController.SetFirstWitnessActorNumber(_iPlayerActorNumber);
            m_vPhotonView.RPC(nameof(ReturnIsPlayerFirstWitnessRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iPlayerActorNumber), _iBodyActorNumber, _iPlayerActorNumber);
        }
    }

    [PunRPC]
    private void ReturnIsPlayerFirstWitnessRPC(int _iBodyActorNumber, int _iPlayerActorNumber)
    {
        PlayerDeadController vPlayerDeadController = MapManager.I.GetPlayerDead(_iBodyActorNumber);

        if (vPlayerDeadController == null)
        {
            return;
        }

        vPlayerDeadController.NotifyDead(_iPlayerActorNumber);

    }
}
