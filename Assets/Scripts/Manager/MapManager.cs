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

    private Dictionary<int, PlayerDead> m_dicPlayerDeadInfo = new Dictionary<int, PlayerDead>();

    public void SpawnWeapons()
    {
        foreach(WeaponSpawn weaponSpawnPoint in weaponSpawnPoints)
        {
            WeaponData vWeaponData = weaponSpawnPoint.GetWeaponToSpawn();

            if (vWeaponData != null)
            {
                object[] vPhotonData = new object[1];
                vPhotonData[0] = vWeaponData.a_iWeaponId;

                PhotonNetwork.InstantiateRoomObject("WeaponPrefab/WeaponBaseObject", weaponSpawnPoint.transform.position, Quaternion.identity, 0, vPhotonData);
            }
        }
    }

    // 플레이어가 사망하면 해당 클라이언트가 MapManager를 호출하고, 호출된 매니저는 서버(마스터클라이언트)를 통해 시체를 생성한다.
    public void SpawnPlayerDeadBody(Vector3 _vPosition, int _iVictimActorNumber, int _iShooterActorNumber, int _iWeaponID, double _dTime)
    {
        m_vPhotonView.RPC(nameof(SpawnPlayerDeadBodyRPC), RpcTarget.MasterClient, _vPosition, _iVictimActorNumber, _iShooterActorNumber, _iWeaponID, _dTime);
    }

    [PunRPC]
    private void SpawnPlayerDeadBodyRPC(Vector3 _vPosition, int _iVictimActorNumber, int _iShooterActorNumber, int _iWeaponID, double _dTime)
    {
        PlayerDead vPlayerDead = PhotonNetwork.InstantiateRoomObject("PlayerDeadBody", _vPosition, Quaternion.identity).GetComponent<PlayerDead>();

        vPlayerDead.InitData(_iVictimActorNumber, GameManager.I.GetPlayerRole(_iVictimActorNumber), _iShooterActorNumber, _iWeaponID, PhotonNetwork.Time);

        m_dicPlayerDeadInfo.Add(_iVictimActorNumber, vPlayerDead);
    }

    //[PunRPC]
    //void SetGameStateRPC(GameObject weapon, Vector3 spawnPoint)
    //{
    //    Instantiate(weapon, spawnPoint, Quaternion.identity);
    //}
}
