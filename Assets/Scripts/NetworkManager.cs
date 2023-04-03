using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

//public class NetworkManager : MonoBehaviourPunCallbacks
public class NetworkManager : SingletonPunCallbacks<NetworkManager>
{
    public InputField NickNameInput;
    public GameObject DisconnectPanel;
    public GameObject RespawnPanel;

    private void Start()    // singleton 적용하기전엔 Awake 함수였슴
    {
        Screen.SetResolution(960, 540, false);

        // 이거 쓰면 더 빨라진다는데 정확힌 모름
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
        StartSetting();
    }

    private void StartSetting()
    {
        DisconnectPanel.SetActive(false);
        StartCoroutine("DestroyBullet");
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
        PhotonNetwork.Instantiate("Player", new Vector3 (Random.Range(-0.5f,1f),Random.Range(-1f,0f),0), Quaternion.identity);
        RespawnPanel.SetActive(false);
    }

    private void Update()
    {
        // ESC 누르면 종료
        if (Input.GetKeyDown(KeyCode.Escape) && PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log(newPlayer.NickName + ": " + newPlayer.ActorNumber.ToString());
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        DisconnectPanel.SetActive(true);
        RespawnPanel.SetActive(false);
    }
}
