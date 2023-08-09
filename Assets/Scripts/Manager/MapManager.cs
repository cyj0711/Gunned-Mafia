using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System;

public class MapManager : SingletonPunCallbacks<MapManager>
{
    public List<WeaponSpawn> weaponSpawnPoints = new List<WeaponSpawn>();

    [SerializeField] Transform m_vDroppedItem;
    public Transform a_vDroppedItem { get { return m_vDroppedItem; } }

    [SerializeField] PhotonView m_vPhotonView;

    private Dictionary<int, PlayerDeadController> m_dicPlayerDeadInfo = new Dictionary<int, PlayerDeadController>();

    private bool m_bIsWeaponSpawned = false;
    public bool a_bIsWeaponSpawned { set { m_bIsWeaponSpawned = value; } }

    private List<GameObject> m_lSpawnedWeaponObject = new List<GameObject>();
    private List<GameObject> m_lSpawnedBodyObject = new List<GameObject>();

    public void InitObjectList()
    {
        m_lSpawnedWeaponObject.Clear();
        m_lSpawnedBodyObject.Clear();
    }

    public void SpawnWeapons()
    {
        if (m_bIsWeaponSpawned)     // 서버 시간차이로 인한 SpawnWeapons 중복 호출을 방지한다.
            return;

        foreach(WeaponSpawn weaponSpawnPoint in weaponSpawnPoints)
        {
            WeaponData vWeaponData = weaponSpawnPoint.GetWeaponToSpawn();

            if (vWeaponData != null)
            {
                object[] vPhotonDataWeapon = new object[1];
                vPhotonDataWeapon[0] = vWeaponData.a_iWeaponId;

                PhotonNetwork.InstantiateRoomObject("WeaponPrefab/WeaponBaseObject", weaponSpawnPoint.transform.position, Quaternion.identity, 0, vPhotonDataWeapon);
            }
        }

        m_bIsWeaponSpawned = true;
    }

    // 플레이어가 사망하면 해당 클라이언트가 MapManager를 호출하고, 호출된 매니저는 서버(마스터클라이언트)를 통해 시체를 생성한다.
    public void SpawnPlayerDeadBody(Vector3 _vPosition, int _iVictimActorNumber, int _iShooterActorNumber, int _iWeaponID, double _dTime, float _fKillerDistance)
    {
        m_vPhotonView.RPC(nameof(SpawnPlayerDeadBodyRPC), RpcTarget.MasterClient, _vPosition, _iVictimActorNumber, _iShooterActorNumber, _iWeaponID, _dTime, _fKillerDistance);
    }

    [PunRPC]
    public void SpawnPlayerDeadBodyRPC(Vector3 _vPosition, int _iVictimActorNumber, int _iShooterActorNumber, int _iWeaponID, double _dTime, float _fKillerDistance)
    {
        object[] vPhotonDataBody = new object[6];
        vPhotonDataBody[0] = _iVictimActorNumber;
        vPhotonDataBody[1] = _iShooterActorNumber;
        vPhotonDataBody[2] = _iWeaponID;
        vPhotonDataBody[3] = _dTime;
        vPhotonDataBody[4] = _fKillerDistance;
        vPhotonDataBody[5] = PhotonNetwork.CurrentRoom.GetPlayer(_iVictimActorNumber).NickName;
        // m_vPhotonView.RPC(nameof(SpawnPlayerDeadBodyRPC), RpcTarget.AllBuffered, _vPosition, _iVictimActorNumber, _iShooterActorNumber, _iWeaponID, _dTime, _fKillerDistance);
        PhotonNetwork.InstantiateRoomObject("PlayerDeadBody", _vPosition, Quaternion.identity, 0, vPhotonDataBody);
    }

    //[PunRPC]
    //private void SpawnPlayerDeadBodyRPC(Vector3 _vPosition, int _iVictimActorNumber, int _iShooterActorNumber, int _iWeaponID, double _dTime, float _fKillerDistance)
    //{
    //    // PlayerDeadController vPlayerDead = PhotonNetwork.InstantiateRoomObject("PlayerDeadBody", _vPosition, Quaternion.identity).GetComponent<PlayerDeadController>();

    //    PlayerDeadController vPlayerDead = Instantiate(DataManager.I.a_vPlayerDeadBodyPrefab, _vPosition, Quaternion.identity).GetComponent<PlayerDeadController>();

    //    vPlayerDead.InitData(_iVictimActorNumber, GameManager.I.GetPlayerRole(_iVictimActorNumber), _iShooterActorNumber, _iWeaponID, _dTime, _fKillerDistance, PhotonNetwork.CurrentRoom.GetPlayer(_iVictimActorNumber).NickName);

    //    m_dicPlayerDeadInfo.Add(_iVictimActorNumber, vPlayerDead);
    //}

    // ActorNumber에 해당하는 유저의 PlayerDeadController 를 가져온다.
    public PlayerDeadController GetPlayerDead(int _iActorNumber)
    {
        if (m_dicPlayerDeadInfo.ContainsKey(_iActorNumber))
            return m_dicPlayerDeadInfo[_iActorNumber];
        else
            return null;
    }

    public void AddPlayerDeadInfo(int _iVictimActorNumber, PlayerDeadController vPlayerDead)
    {
        m_dicPlayerDeadInfo.Add(_iVictimActorNumber, vPlayerDead);
    }

    //[PunRPC]
    //void SetGameStateRPC(GameObject weapon, Vector3 spawnPoint)
    //{
    //    Instantiate(weapon, spawnPoint, Quaternion.identity);
    //}
}
