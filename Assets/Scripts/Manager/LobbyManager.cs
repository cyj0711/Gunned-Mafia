using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Text connectionInfoText;
    public Button joinButton;
    public InputField NickNameInput;

    private void Start()
    {
        //Screen.SetResolution(960, 540, false);

        // 이거 쓰면 더 빨라진다는데 정확힌 모름
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        PhotonNetwork.ConnectUsingSettings();   // 여기서 connect가 완료되면 OnConnectedToMaster() 함수가 자동으로 호출된다.

        joinButton.interactable = false;
        connectionInfoText.text = "Connecting to Master Server...";
    }

    public override void OnConnectedToMaster()
    {
        joinButton.interactable = true;
        connectionInfoText.text = "Online : Connected to Master Server";
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        joinButton.interactable = false;
        connectionInfoText.text = $"Offline : Connection Disabled {cause.ToString()} - Try reconnecting...";

        PhotonNetwork.ConnectUsingSettings();   // 접속에 실패해도 재접속 시도
    }

    public void Connect()
    {
        joinButton.interactable = false;    // 중복 접속 시도 차단

        if (PhotonNetwork.IsConnected)
        {
            connectionInfoText.text = "Connecting to Random Room...";
            PhotonNetwork.JoinRandomRoom(); // 빈 방을 찾는데 실패하면 OnJoinRandomFailed() 함수 자동 호출
        }
        else
        {
            connectionInfoText.text = "Offline : Connection Disabled - Try reconnecting...";

            PhotonNetwork.ConnectUsingSettings();   // 접속에 실패해도 재접속 시도
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        connectionInfoText.text = "There is no empty room, Creating new Room.";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8 });
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
        connectionInfoText.text = "Connected with Room.";
        PhotonNetwork.LoadLevel("GamePlay");    // SceneManager.LoadScene을 사용하면 각 유저가 개별의 씬을 로드하기때문에(동기화가 안됨) 사용하면안됨

    }
}
