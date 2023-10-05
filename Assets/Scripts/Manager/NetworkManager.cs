using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class NetworkManager : SingletonPunCallbacks<NetworkManager>
{
    [SerializeField] private PhotonView m_vPhotonView;

    private void Start()    // singleton 적용하기전엔 Awake 함수였슴
    {
        //Screen.SetResolution(960, 540, false);

        // 이거 쓰면 더 빨라진다는데 정확힌 모름
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        StartSetting();
    }

    private void StartSetting()
    {
        //DisconnectPanel.SetActive(false);
        //StartCoroutine("DestroyBullet");
        Spawn();
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-0.5f, 1f), Random.Range(-1f, 0f), 0), Quaternion.identity);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if(newPlayer!=PhotonNetwork.LocalPlayer)
        {
            GameObject tagObject = (GameObject)PhotonNetwork.LocalPlayer.TagObject;
            tagObject.GetComponent<PlayerController>().InvokeProperties();
            tagObject.GetComponentInChildren<WeaponController>().InvokeProperties();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        PlayerController vPlayerController = GameManager.I.GetPlayerController(otherPlayer.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            ChatManager.I.SendChat(E_ChatType.System, GameManager.I.GetPlayerNickName(otherPlayer.ActorNumber) + " left the game.");
            // 살아있는 플레이어가 나가면 해당 플레이어의 시체를 소환한다.
            if (!MapManager.I.a_dicPlayerDead.ContainsKey(otherPlayer.ActorNumber) && vPlayerController.a_ePlayerState == E_PlayerState.Alive)
            {
                // 시스템상의 자살이나 게임종료로 인한 사망은 ShooterActorNumber, WeaponID, killerDistance 를 전부 0으로 표시한다.
                MapManager.I.SpawnPlayerDeadBody(vPlayerController.transform.position, otherPlayer.ActorNumber, 0, 0, PhotonNetwork.Time, 0f);

                // a_ePlayerState = E_PlayerState.Missing;
                GameManager.I.CheckGameOver(otherPlayer.ActorNumber);
            }
        }

        if (vPlayerController.a_vWeaponController != null)
            vPlayerController.a_vWeaponController.DropAllWeaponsOnLeft();

        GameManager.I?.RemovePlayerController(otherPlayer.ActorNumber);
        UIScoreBoardManager.I?.RemoveScoreBoardItem(otherPlayer.ActorNumber);

    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //DisconnectPanel.SetActive(true);
        //RespawnPanel.SetActive(false);
    }

    public void LeaveGame()
    {
        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("LogIn");
    }

    // TODO: 마스터가 나가서 새 마스터가 생기면 할일(메뉴의 Room Setting 버튼 활성화 라거나)
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if(newMasterClient.IsLocal)
        {
            GameManager.I.MasterClientSwitchedProcess();
            UIMenuManager.I.MasterClientSwitchedProcess();
            ChatManager.I.SendChat(E_ChatType.System, "The host has been changed to " + newMasterClient.NickName);
        }
        UIScoreBoardManager.I.MasterClientSwitchedProcess(newMasterClient.ActorNumber);
    }
}
