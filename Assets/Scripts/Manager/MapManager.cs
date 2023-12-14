using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System;

public class MapManager : Singleton<MapManager>
{
    public List<WeaponSpawn> weaponSpawnPoints = new List<WeaponSpawn>();

    [SerializeField] Transform m_vDroppedItem;
    public Transform a_vDroppedItem { get { return m_vDroppedItem; } }

    [SerializeField] PhotonView m_vPhotonView;

    private Dictionary<int, PlayerDeadController> m_dicPlayerDead = new Dictionary<int, PlayerDeadController>();
    public IReadOnlyDictionary<int, PlayerDeadController> a_dicPlayerDead => m_dicPlayerDead;

    private bool m_bIsWeaponSpawned = false;

    private List<WeaponBase> m_listSpawnedWeaponBase = new List<WeaponBase>();

    private Dictionary<int, PlayerDeadController> m_dicNearBody = new Dictionary<int, PlayerDeadController>();  // 로컬 플레이어와 가까이 있는(조사 가능한) 시체
    private PlayerDeadController m_vNearestBody = null;
    public PlayerDeadController a_vNearestBody { get => m_vNearestBody; }
    private Coroutine m_coFindNearestBody = null;

    public void InitVariable()
    {
        m_bIsWeaponSpawned = false;
    }

    // 게임이 prepare 상태가 되면 맵에 무기들을 스폰한다.
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

                switch (vWeaponData.a_eWeaponType)
                {
                    case E_WeaponType.Shotgun:
                        PhotonNetwork.InstantiateRoomObject("WeaponPrefab/Weapon Shotgun", weaponSpawnPoint.transform.position, Quaternion.identity, 0, vPhotonDataWeapon);
                        break;
                    default:
                        PhotonNetwork.InstantiateRoomObject("WeaponPrefab/Weapon Normal", weaponSpawnPoint.transform.position, Quaternion.identity, 0, vPhotonDataWeapon);
                        break;
                }
            }
        }

        m_bIsWeaponSpawned = true;
    }

    public void AddListWeapon(WeaponBase _vWeaponBase)
    {
        m_listSpawnedWeaponBase.Add(_vWeaponBase);
    }

    // 게임이 waiting 상태가 되면 이때까지 생성된 모든 무기를 없앤다.
    public void RemoveAllWeapons()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            foreach (WeaponBase _vWeaponBase in m_listSpawnedWeaponBase)
                PhotonNetwork.Destroy(_vWeaponBase.gameObject);
        }
        m_listSpawnedWeaponBase.Clear();
    }

    // 게임이 waiting 상태가 되면 이때까지 생성된 모든 시체를 없앤다.
    public void RemoveAllBodies()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            foreach (KeyValuePair<int,PlayerDeadController>kvPair  in m_dicPlayerDead)
                PhotonNetwork.Destroy(kvPair.Value.gameObject);
        }
        m_dicPlayerDead.Clear();
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
        vPhotonDataBody[5] = GameManager.I.GetPlayerNickName(_iVictimActorNumber);
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
        if (m_dicPlayerDead.ContainsKey(_iActorNumber))
            return m_dicPlayerDead[_iActorNumber];
        else
            return null;
    }

    public void AddDictionaryPlayerDead(int _iVictimActorNumber, PlayerDeadController vPlayerDead)
    {
        if (!m_dicPlayerDead.ContainsKey(_iVictimActorNumber))
            m_dicPlayerDead.Add(_iVictimActorNumber, vPlayerDead);
    }

    // 시체와 로컬 플레이어가 충돌(trigger)하면 해당 시체를 dicCloseBody에 추가한다.
    public void AddDictionaryNearBody(int _iVictimActorNumber, PlayerDeadController vPlayerDead)
    {
        if (!m_dicNearBody.ContainsKey(_iVictimActorNumber))
            m_dicNearBody.Add(_iVictimActorNumber, vPlayerDead);

        // 거리를 통해 가장 가까운 시체를 찾는건 모바일 기기에서만 동작한다(pc에서는 마우스를 올린 곳이 가장 가까운 시체로 처리)
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            // 가장 가까운 시체가 없으면 해당 시체를 가장 가까운 시체로 지정한다.
            if (m_vNearestBody == null)
            {
                SetNearestBody(vPlayerDead.a_iVictimActorNumber);
            }
            // 가장 가까운 시체가 있다면,  그중 가장 가까운 시체를 찾도록 coroutine을 돌린다.
            else
            {
                if (m_coFindNearestBody == null)
                    m_coFindNearestBody = StartCoroutine(FindNearestBodyCoroutine());
            }
        }
    }

    // 시체와 로컬 플레이어가 충돌(trigger)에서 벗어나면 dicCloseBody에 해당 시체를 지운다.
    public void RemoveDictionaryNearBody(int _iVictimActorNumber)
    {
        if (m_dicNearBody.ContainsKey(_iVictimActorNumber))
            m_dicNearBody.Remove(_iVictimActorNumber);

        if (m_vNearestBody != null && _iVictimActorNumber == m_vNearestBody.a_iVictimActorNumber)
            SetNearestBody(-1);

        // 거리를 통해 가장 가까운 시체를 찾는건 모바일 기기에서만 동작한다(pc에서는 마우스를 올린 곳이 가장 가까운 시체로 처리)
        if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.Android)
        {
            // 인접한 시체가 없으면 가장 가까운 시체를 찾는 코루틴을 종료한다.
            if (m_dicNearBody.Count <= 1)
            {
                StopCoroutine(FindNearestBodyCoroutine());
                m_coFindNearestBody = null;

                // 가까운 시체가 아무것도 없으면 가장 가까운 시체를 null로 하고, 하나만 있으면 해당 시체를 가장 가까운 시체로 한다.
                if (m_dicNearBody.Count == 0)
                    SetNearestBody(-1);
                else
                {
                    foreach(KeyValuePair<int, PlayerDeadController> _kvPair in m_dicNearBody)
                    {
                        SetNearestBody(_kvPair.Key);
                        break;
                    }
                }
            }
        }
    }

    public PlayerDeadController GetNearBody(int _iVictimActorNumber)
    {
        if (m_dicNearBody.ContainsKey(_iVictimActorNumber))
            return m_dicNearBody[_iVictimActorNumber];
        else
            return null;
    }

    // 가장 가까운 시체 세팅 (시체 조사 가능 ui 포함)
    public void SetNearestBody(int _iVictimActorNumber)
    {
        // 가장 가까운 시체를 설정하기전 그 이전의 가장 가까운 시체의 ui창을 비활성화한다.
        if (m_vNearestBody != null)
        {
            m_vNearestBody.ActivateSearch(false);
        }

        if (_iVictimActorNumber == -1)
        {
            m_vNearestBody = null;
            return;
        }

        if (m_dicNearBody.ContainsKey(_iVictimActorNumber))
        {
            m_vNearestBody = m_dicNearBody[_iVictimActorNumber];
            m_vNearestBody.ActivateSearch(true);
        }
    }

    private IEnumerator FindNearestBodyCoroutine()
    {
        while(m_vNearestBody!=null)
        {
            foreach(KeyValuePair<int, PlayerDeadController> _kvPair in m_dicNearBody)
            {
                if (m_vNearestBody != _kvPair.Value)
                {
                    if (Vector3.Distance(_kvPair.Value.transform.position, GameManager.I.GetPlayerController().transform.position)
                        < Vector3.Distance(m_vNearestBody.transform.position, GameManager.I.GetPlayerController().transform.position))
                    {
                        SetNearestBody(_kvPair.Key);
                    }
                }
            }

            yield return null;
        }
    }

    //[PunRPC]
    //void SetGameStateRPC(GameObject weapon, Vector3 spawnPoint)
    //{
    //    Instantiate(weapon, spawnPoint, Quaternion.identity);
    //}
}
