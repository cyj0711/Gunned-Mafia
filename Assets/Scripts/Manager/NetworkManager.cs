using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//public class NetworkManager : MonoBehaviourPunCallbacks
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

    //// 인게임에서 접속 버튼 누르면 Connect 함수 작동
    //public void Connect() => PhotonNetwork.ConnectUsingSettings();

    //public override void OnConnectedToMaster()
    //{
    //    PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    //    PhotonNetwork.JoinOrCreateRoom("Room", new RoomOptions { MaxPlayers = 8 }, null);
    //}

    //public override void OnJoinedRoom()
    //{
    //    DisconnectPanel.SetActive(false);
    //    StartCoroutine("DestroyBullet");
    //    Spawn();
    //}

    IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(0.2f);
        foreach (GameObject GO in GameObject.FindGameObjectsWithTag("Bullet"))
            GO.GetComponent<PhotonView>().RPC("DestroyRPC", RpcTarget.All);
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate("Player", new Vector3(Random.Range(-0.5f, 1f), Random.Range(-1f, 0f), 0), Quaternion.identity);
    }

    //private void Update()
    //{
    //    // ESC 누르면 종료
    //    if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
    //        PhotonNetwork.Disconnect();
    //}

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

       // int[] listOwnedWeaponViewid = vPlayerController.a_vWeaponController.GetOwnedWeaponPhotonViewID();

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


        // 나간 플레이어의 무기 떨구기

        //float fDropRotation = 360f / listOwnedWeaponViewid.Length;
        //int index = 0;

        //foreach (int _iViewID in listOwnedWeaponViewid)
        //{
        //    PhotonView vWeaponPhotonView = PhotonView.Find(_iViewID);
        //    if (vWeaponPhotonView == null || vWeaponPhotonView.gameObject == null) { continue; }

        //    WeaponBase vCurrentWeapon = vWeaponPhotonView.GetComponent<WeaponBase>();

        //    vCurrentWeapon.InitWeaponData(vCurrentWeapon.a_iCurrentAmmo, vCurrentWeapon.a_iRemainAmmo);

        //    vCurrentWeapon.transform.parent = MapManager.I.a_vDroppedItem;
        //    vCurrentWeapon.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        //    if (vCurrentWeapon.gameObject.transform.localScale.y < 0)
        //        vCurrentWeapon.gameObject.transform.localScale = new Vector3
        //            (vCurrentWeapon.gameObject.transform.localScale.x, vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);

        //    vCurrentWeapon.gameObject.SetActive(true);

        //    vCurrentWeapon.DropWeapon(Quaternion.Euler(new Vector3(0f, 0f, index * fDropRotation)));
        //    index++;
        //}
        //// m_vWeaponController.DropAllWeaponsOnLeft();

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
        //PlayerController vPlayerController = GameManager.I.GetPlayerController();

        //m_vPhotonView.RPC(nameof(PlayerLeftRPC), RpcTarget.All,
        //    PhotonNetwork.LocalPlayer.ActorNumber, (int)vPlayerController.a_ePlayerState, vPlayerController.transform.position, (object)vPlayerController.a_vWeaponController.GetOwnedWeaponPhotonViewID());

        PhotonNetwork.LeaveRoom();

        SceneManager.LoadScene("LogIn");
    }

    //[PunRPC]
    //public void PlayerLeftRPC(int _iActorNumber, int _iPlayerState, Vector3 vPlayerPosition, object objListOwnedWeaponViewID)
    //{
    //    int[] listOwnedWeaponViewid = (int[])objListOwnedWeaponViewID;

    //    GameManager.I?.RemovePlayerController(_iActorNumber);
    //    UIScoreBoardManager.I?.RemoveScoreBoardItem(_iActorNumber);

    //   // PhotonNetwork.SetMasterClient();

    //    if (PhotonNetwork.IsMasterClient)
    //    {
    //        ChatManager.I.SendChat(E_ChatType.System, GameManager.I.GetPlayerNickName(_iActorNumber) + " left the game.");
    //        // 살아있는 플레이어가 나가면 해당 플레이어의 시체를 소환한다.
    //        if (!MapManager.I.a_dicPlayerDead.ContainsKey(_iActorNumber) && (E_PlayerState)_iPlayerState == E_PlayerState.Alive)
    //        {
    //            // 시스템상의 자살이나 게임종료로 인한 사망은 ShooterActorNumber, WeaponID, killerDistance 를 전부 0으로 표시한다.
    //            MapManager.I.SpawnPlayerDeadBody(vPlayerPosition, _iActorNumber, 0, 0, PhotonNetwork.Time, 0f);
    //            // a_ePlayerState = E_PlayerState.Missing;
    //            GameManager.I.CheckGameOver(_iActorNumber);
    //        }
    //    }

    //    // 나간 플레이어의 무기 떨구기
    //    float fDropRotation = 360f / listOwnedWeaponViewid.Length;
    //    int index = 0;

    //    foreach (int _iViewID in listOwnedWeaponViewid)
    //    {
    //        PhotonView vWeaponPhotonView = PhotonView.Find(_iViewID);
    //        if (vWeaponPhotonView == null || vWeaponPhotonView.gameObject == null) { continue; }

    //        WeaponBase vCurrentWeapon = vWeaponPhotonView.GetComponent<WeaponBase>();

    //        vCurrentWeapon.InitWeaponData(vCurrentWeapon.a_iCurrentAmmo, vCurrentWeapon.a_iRemainAmmo);

    //        vCurrentWeapon.transform.parent = MapManager.I.a_vDroppedItem;
    //        vCurrentWeapon.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    //        if (vCurrentWeapon.gameObject.transform.localScale.y < 0)
    //            vCurrentWeapon.gameObject.transform.localScale = new Vector3
    //                (vCurrentWeapon.gameObject.transform.localScale.x, vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);

    //        vCurrentWeapon.gameObject.SetActive(true);

    //        vCurrentWeapon.DropWeapon(Quaternion.Euler(new Vector3(0f, 0f, index * fDropRotation)));
    //        index++;
    //    }
    //    // m_vWeaponController.DropAllWeaponsOnLeft();
    //}

}
