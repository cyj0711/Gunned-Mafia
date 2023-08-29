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
    public TMP_InputField a_vNickNameInputField { get => m_vNickNameInputField; }

    [SerializeField] TMP_InputField m_vRoomNameInputField;
    [SerializeField] TMP_InputField m_vMaxPlayerInputField;
    [SerializeField] TMP_InputField m_vRoomPasswordInputField;

    [SerializeField] Button m_vCreateButton;

    private Dictionary<string, RoomData> m_dicRoomData = new Dictionary<string, RoomData>();

    [SerializeField] GameObject m_vRoomEntityPrefab;
    [SerializeField] Transform m_vRoomListContent;

    [SerializeField] GameObject m_vNickNameInvalidMessageObject;

    [SerializeField] GameObject m_vNoRoomTextObject;

    void Start()
    {
        PhotonNetwork.JoinLobby();
        // m_vMaxPlayerInputField.onEndEdit.AddListener(CheckValidMaxPlayer);
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
            else if(m_dicRoomData.ContainsKey(_room.Name)==false)   // 새 방 추가됨
            {
                RoomData vRoomData = Instantiate(m_vRoomEntityPrefab, m_vRoomListContent).GetComponent<RoomData>();
                vRoomData.SetRoomInfo(_room);
                m_dicRoomData.Add(_room.Name, vRoomData);
            }
            else // 기존 방 정보 갱신
            {
                m_dicRoomData[_room.Name].SetRoomInfo(_room);
            }
        }

        if(roomList.Count>0)
        {
            m_vNoRoomTextObject.SetActive(false);
        }
        else
        {
            m_vNoRoomTextObject.SetActive(true);
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
        if (m_vNickNameInputField.text == "")
        {
            SetActiveNickNameInvalidMessage(true);
            return;
        }

        RoomOptions roomOption = new RoomOptions();
        roomOption.MaxPlayers = byte.Parse(m_vMaxPlayerInputField.text);
        roomOption.IsOpen = true; //방이 열려있는지 닫혀있는지 설정
        roomOption.IsVisible = true; //비공개 방 여부

        PhotonNetwork.CreateRoom(m_vRoomNameInputField.text, roomOption, null);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = m_vNickNameInputField.text;

        PhotonNetwork.LoadLevel("GamePlay");    // SceneManager.LoadScene을 사용하면 각 유저가 개별의 씬을 로드하기때문에(동기화가 안됨) 사용하면안됨

    }

    // Max Player TMP Input 에서 숫자 입력이 끝났을 때, 해당 숫자가 게임 가용 인원(2~16)에 맞는지 확인
    public void CheckValidMaxPlayer(string _strInput)
    {
        if (_strInput == "") return;

        m_vMaxPlayerInputField.text = Mathf.Clamp(int.Parse(_strInput), 2, 16).ToString();
    }

    // 방의 이름과 최대 인원수를 정해야만 방을 만들 수 있슴
    public void CheckValidCreateRoom()
    {
        if (m_vRoomNameInputField.text != "" && m_vMaxPlayerInputField.text != "")
        {
            m_vCreateButton.interactable = true;
        }
        else
        {
            m_vCreateButton.interactable = false;
        }
    }

    // 닉네임을 설정하지 않을 경우 닉네임 설정 경고창 활성화
    public void SetActiveNickNameInvalidMessage(bool _bIsActive)
    {
        m_vNickNameInvalidMessageObject.SetActive(_bIsActive);
    }
}
