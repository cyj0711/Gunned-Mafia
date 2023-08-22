using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : SingletonPunCallbacks<LobbyManager>
{
    [SerializeField] GameObject m_vCreateRoomPanelObject;
    [SerializeField] GameObject m_vRoomListScrollViewObject;

    [SerializeField] TMP_InputField m_vNickNameInputField;

    [SerializeField] TMP_InputField m_vRoomNameInputField;
    [SerializeField] TMP_InputField m_vMaxPlayerInputField;
    [SerializeField] TMP_InputField m_vRoomPasswordInputField;

    private Dictionary<string, RoomData> m_dicRoomData = new Dictionary<string, RoomData>();

    [SerializeField] GameObject m_vRoomEntityPrefab;
    [SerializeField] Transform m_vRoomListContent;

    void Start()
    {
        PhotonNetwork.JoinLobby();
        m_vMaxPlayerInputField.onEndEdit.AddListener(CheckValidMaxPlayer);
    }

    public override void OnJoinedLobby()
    {
        m_vRoomListScrollViewObject.SetActive(true);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo _room in roomList)
        {
            // 방이 삭제됨
            if (_room.RemovedFromList == true)
            {
                Destroy(m_dicRoomData[_room.Name].gameObject);
            }
            else if(m_dicRoomData.ContainsKey(_room.Name)==false)
            {
                RoomData vRoomData = Instantiate(m_vRoomEntityPrefab, m_vRoomListContent).GetComponent<RoomData>();
                vRoomData.SetRoomInfo(_room);
                m_dicRoomData.Add(_room.Name, vRoomData);
            }
            else
            {
                m_dicRoomData[_room.Name].SetRoomInfo(_room);
            }
        }
    }

    // Activate by Create Room Button
    public void OpenCreateRoomPanel()
    {
        m_vCreateRoomPanelObject.SetActive(true);
    }

    // Activate by Cancel Button
    public void CloseCreateRoomPanel()
    {
        m_vCreateRoomPanelObject.SetActive(false);
    }

    // Activate by Create Button
    public void CreateRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = m_vNickNameInputField.text;

        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = byte.Parse(m_vMaxPlayerInputField.text);
        roomOption.IsOpen = true; //방이 열려있는지 닫혀있는지 설정
        roomOption.IsVisible = true; //비공개 방 여부

        PhotonNetwork.CreateRoom(m_vRoomNameInputField.text, roomOption, null);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("GamePlay");    // SceneManager.LoadScene을 사용하면 각 유저가 개별의 씬을 로드하기때문에(동기화가 안됨) 사용하면안됨

    }

    // Activate by Max Player TMP Input
    public void CheckValidMaxPlayer(string _strInput)
    {
        m_vMaxPlayerInputField.text = Mathf.Clamp(int.Parse(_strInput), 2, 16).ToString();
    }
}
