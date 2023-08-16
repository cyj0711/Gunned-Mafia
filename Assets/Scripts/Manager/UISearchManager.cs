using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISearchManager : Singleton<UISearchManager>
{
    [SerializeField] private PhotonView m_vPhotonView;

    [SerializeField] private GameObject m_vSearchPanelObject;
    [SerializeField] private Text m_vSearchText;
    [SerializeField] private Button m_vTrackDnaButton;
    [SerializeField] private Button m_vCallDetectiveButton;

    int m_iVictimActorNumber = -1;   // 시체 탐색 ui에서 해당 시체 주인이 누구인지 확인(call detective 버튼으로 탐정에게 위치를 보내는 용도)
    int m_iKillerActorNumber = -1;  // 탐정이 시체를 탐색하고 dna를 추적할 때 살인자의 위치를 찾는 용도
    int m_iRemainTimeDNA;

    private Dictionary<int, LocationPingController> m_dicPlayerLocationPing = new Dictionary<int, LocationPingController>();    // 플레이어(죽었으면 시체)의 위치를 표시하는 핑들을 저장. 탐정 ui 용도.
    public IReadOnlyDictionary<int, LocationPingController> a_dicPlayerLocationPing => m_dicPlayerLocationPing;

    public void SetSearchPanelActive(bool _bIsActive)
    {
        m_vSearchPanelObject.SetActive(_bIsActive);

        if (_bIsActive == false)
        {
            m_iVictimActorNumber = -1;
            m_iKillerActorNumber = -1;
        }
    }

    public void SetSearchText(int _iVictim, int _iKiller, string _strVictimNickName, E_PlayerRole _eVictimRole, int _iWeapon, int _iDeadTime, int _iRemainTimeDNA)
    {
        // string sText = "This is the body of ''. His role is ''! He was killed by a ''. It's been '' seconds since he died.";
        string sText =
            "This is the body of " + _strVictimNickName +
            ". His role is " + _eVictimRole +
            ". He was killed by a " + DataManager.I.GetWeaponDataWithID(_iWeapon).a_strWeaponName +
            ". It's been " + _iDeadTime / 60 + " minutes and " + _iDeadTime % 60 + " seconds since he died." +
            " DNA Data remains " + _iRemainTimeDNA / 60 + " minutes and " + _iRemainTimeDNA % 60 + " seconds.";

        m_vSearchText.text = sText;
        m_iVictimActorNumber = _iVictim;
        m_iKillerActorNumber = _iKiller;
        m_iRemainTimeDNA = _iRemainTimeDNA;

        if (_iRemainTimeDNA > 0)
        {
            m_vTrackDnaButton.interactable = true;
        }
        else
        {
            m_vTrackDnaButton.interactable = false;
        }


        RemoveDicPlayerLocationPing(_iVictim);  // 탐정이 시체를 조사하면, 해당 시체에 호출된 핑을 제거한다.
    }

    // 시체 조사 창에서 call detective 버튼을 누르면 해당 함수를 호출하여 탐정 플레이어들에게 시체의 위치를 알려준다.
    public void CallDetective()
    {
        if (m_iVictimActorNumber == -1) return;

        List<int> listDetectivePlayers = GameManager.I.GetDetectivePlayers();

        PlayerDeadController vDeadPlayer = MapManager.I.GetPlayerDead(m_iVictimActorNumber);

        if (vDeadPlayer == null)
            return;

        PlayerController vDetectivePlayerController;

        foreach (int indexDetectivePlayers in listDetectivePlayers)
        {
            vDetectivePlayerController = GameManager.I.GetPlayerController(indexDetectivePlayers);

            if (vDetectivePlayerController != null && vDetectivePlayerController.a_ePlayerState == E_PlayerState.Alive)
                DisplayLocation(indexDetectivePlayers, vDeadPlayer.a_vPhotonView.ViewID, 30f);
        }
    }

    // detective 플레이어가 시체를 조사했을때 track dna 버튼을 누르면 호출됨. 범인의 위치를 알수있다.
    public void TrackKiller()
    {
        if (m_iKillerActorNumber == -1) return;

        PlayerController vKillerPlayer = GameManager.I.GetPlayerController(m_iKillerActorNumber);

        Transform vKillerTransform;

        if (vKillerPlayer == null || vKillerPlayer.a_ePlayerState != E_PlayerState.Alive)    // 범인이 죽었거나 게임에서 나갔으면 범인의 시체라도 확인
        {
            PlayerDeadController vKillerPlayerBody = MapManager.I.GetPlayerDead(m_iKillerActorNumber);

            if (vKillerPlayerBody == null)  // 범인의 시체마저 존재하지않으면 추적할 방법이 없으므로 return
            {
                UIGameManager.I.SendNotification("Can't track the killer...");
                return;
            }
            vKillerTransform = vKillerPlayerBody.transform;
        }
        else
        {
            vKillerTransform = vKillerPlayer.transform;
        }

        DisplayLocation(vKillerTransform, (float)m_iRemainTimeDNA, true);
    }

    public void DisplayLocation(Transform _vTargetTransform, float _fDisplayTime, bool _bIsUpdatingPing = false)
    {
        PlayerDeadController vPlayerDeadController = _vTargetTransform.GetComponent<PlayerDeadController>();
        if (vPlayerDeadController != null)  // 이미 해당 시체를 호출한 상태이면 중복 호출을 스킵함
        {
            if (m_dicPlayerLocationPing.ContainsKey(vPlayerDeadController.a_iVictimActorNumber))
            {
                return;
            }
        }

        GameObject vLocationPingObject = Instantiate(DataManager.I.a_vLocationPingPrefab);
        LocationPingController vLocationPingController = vLocationPingObject.GetComponent<LocationPingController>();

        if (vLocationPingObject == null)
        {
            Destroy(vLocationPingObject);
            return;
        }

        vLocationPingController.transform.position = _vTargetTransform.position;
        vLocationPingController.InitData(GameManager.I.GetPlayerController().transform, _vTargetTransform, _fDisplayTime, _bIsUpdatingPing);

        int iActorNumber = -1;

        // 추적 핑이 시체를 추적하는지 살아있는 플레이어를 추적하는지 확인
        if (vPlayerDeadController != null)
        {
            iActorNumber = vPlayerDeadController.a_iVictimActorNumber;
        }
        else
        {
            PlayerController vPlayerController = _vTargetTransform.GetComponent<PlayerController>();

            if(vPlayerController!=null)
            {
                iActorNumber = vPlayerController.a_vPhotonView.OwnerActorNr;
            }
        }

        vLocationPingController.SetTargetPlayerActorNumber(iActorNumber);

        m_dicPlayerLocationPing.Add(iActorNumber, vLocationPingController);
    }

    // _iPlayerActorNumber 플레이어의 화면에 _vTargetPosition 의 위치를 _fDisplayTime 동안 표시한다.
    public void DisplayLocation(int _iPlayerActorNumber, int _iTargetViewID, float _fDisplayTime)
    {
        m_vPhotonView.RPC(nameof(DisplayLocationRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iPlayerActorNumber), _iTargetViewID, _fDisplayTime);
    }

    [PunRPC]
    private void DisplayLocationRPC(int _iTargetViewID, float _fDisplayTime)
    {
        DisplayLocation(PhotonView.Find(_iTargetViewID).transform, _fDisplayTime);
    }

    // 위치 추적 핑이 시간이 사라지면 dictionary 에서도 제거한다.
    public void RemoveDicPlayerLocationPing(int _iKey)
    {
        if (!m_dicPlayerLocationPing.ContainsKey(_iKey))
            return;

        LocationPingController vLocationPingController = m_dicPlayerLocationPing[_iKey];

        m_dicPlayerLocationPing.Remove(_iKey);
        vLocationPingController.SetTargetPlayerActorNumber(-1);

        Destroy(vLocationPingController.gameObject);
    }

    public void RemoveDicPlayerLocationPingAll()
    {
        foreach(KeyValuePair<int, LocationPingController>kvPair in m_dicPlayerLocationPing)
        {
            kvPair.Value.SetTargetPlayerActorNumber(-1);
            Destroy(kvPair.Value.gameObject);
        }
        m_dicPlayerLocationPing.Clear();
    }

    // dna 추적 중에 추적당한 플레이어가 사망하면 해당 플레이어의 시체를 대신 표시한다.
    public void ChangeTrackedPlayerToBody(int _iDeadPlayerActorNumber)
    {
        if (!m_dicPlayerLocationPing.ContainsKey(_iDeadPlayerActorNumber))
            return;

        LocationPingController vLocationPingController = a_dicPlayerLocationPing[_iDeadPlayerActorNumber];
        PlayerDeadController vPlayerDeadController = MapManager.I.GetPlayerDead(_iDeadPlayerActorNumber);

        if (vLocationPingController != null && vPlayerDeadController != null)
        {
            vLocationPingController.SetTargetTransform(vPlayerDeadController.transform);
        }
        else
        {
            RemoveDicPlayerLocationPing(_iDeadPlayerActorNumber);
        }
    }
}
