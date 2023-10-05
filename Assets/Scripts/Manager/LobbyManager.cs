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
    [SerializeField] TMP_InputField m_vNumberOfMafiaInputField;
    [SerializeField] TMP_InputField m_vNumberOfDetectiveInputField;

    [SerializeField] Toggle m_vAutoRoleToggle;

    [SerializeField] Button m_vCreateButton;

    private Dictionary<string, RoomData> m_dicRoomData = new Dictionary<string, RoomData>();

    [SerializeField] GameObject m_vRoomEntityPrefab;
    [SerializeField] Transform m_vRoomListContent;

    [SerializeField] GameObject m_vNickNameInvalidMessageObject;

    [SerializeField] GameObject m_vNoRoomTextObject;

    int m_iMaxPlayer = -1;
    int m_iNumberOfMafia = -1;
    int m_iNumberOfDetective = -1;

    void Start()
    {
        //m_dicRoomData.Clear();
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
                if (m_dicRoomData.ContainsKey(_room.Name))
                {
                    Destroy(m_dicRoomData[_room.Name].gameObject);
                    m_dicRoomData.Remove(_room.Name);
                }
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

        if(m_dicRoomData.Count>0)
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
        roomOption.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "IsAutoRole", m_vAutoRoleToggle.isOn }, { "NumberOfMafia", m_iNumberOfMafia }, { "NumberOfDetective", m_iNumberOfDetective } };

        PhotonNetwork.CreateRoom(m_vRoomNameInputField.text, roomOption, null);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = m_vNickNameInputField.text;

        PhotonNetwork.LoadLevel("GamePlay");    // SceneManager.LoadScene을 사용하면 각 유저가 개별의 씬을 로드하기때문에(동기화가 안됨) 사용하면안됨

    }

    // Max Player TMP Input 에서 숫자 입력이 끝났을 때, 해당 숫자가 게임 가용 인원(2~16)에 맞는지 확인
    public void OnEndEditMaxPlayer(string _strInput)
    {
        if (_strInput == "")
        {
            ((TMP_Text)m_vNumberOfMafiaInputField.placeholder).text = "Need the max player";
            ((TMP_Text)m_vNumberOfDetectiveInputField.placeholder).text = "Need the max player";
            return;
        }

        m_iMaxPlayer= Mathf.Clamp(int.Parse(_strInput), 2, 16);
        m_vMaxPlayerInputField.text = m_iMaxPlayer.ToString();

        ((TMP_Text)m_vNumberOfMafiaInputField.placeholder).text = "1 ~ " + (m_iMaxPlayer - 1);
        ((TMP_Text)m_vNumberOfDetectiveInputField.placeholder).text = "0 ~ " + (m_iMaxPlayer - 1);
    }


    public void EndEditMafiaInputField(string _strInput)
    {
        if (_strInput == "") return;

        m_iNumberOfMafia = Mathf.Clamp(int.Parse(_strInput), 1, m_iMaxPlayer - 1);
        m_vNumberOfMafiaInputField.text = m_iNumberOfMafia.ToString();

        // 만약 마피아 수 + 탐정 수가 최대 인원을 넘어서면 넘어선만큼 탐정 수를 조정한다.
        if (m_vNumberOfDetectiveInputField.text != "")
        {
            if (m_iNumberOfMafia + m_iNumberOfDetective > m_iMaxPlayer)
            {
                m_vNumberOfDetectiveInputField.text = (m_iMaxPlayer - m_iNumberOfMafia).ToString();
            }
        }
    }

    public void EndEditDetectiveInputField(string _strInput)
    {
        if (_strInput == "") return;

        m_iNumberOfDetective = Mathf.Clamp(int.Parse(_strInput), 0, m_iMaxPlayer - 1);
        m_vNumberOfDetectiveInputField.text = m_iNumberOfDetective.ToString();

        // 만약 탐정 수 + 마피아 수가 최대 인원을 넘어서면 넘어선만큼 마피아 수를 조정한다.
        if (m_vNumberOfMafiaInputField.text != "")
        {
            if (m_iNumberOfDetective + m_iNumberOfMafia > m_iMaxPlayer)
            {
                m_vNumberOfMafiaInputField.text = (m_iMaxPlayer - m_iNumberOfDetective).ToString();
            }
        }
    }

    public void OnValueChangedAutoRoleSetting(bool _bValue)
    {
        m_vNumberOfMafiaInputField.interactable = !_bValue;
        m_vNumberOfDetectiveInputField.interactable = !_bValue;

        m_vNumberOfMafiaInputField.text = "";
        m_vNumberOfDetectiveInputField.text = "";
    }


    // 방의 이름과 최대 인원수를 정해야만 방을 만들 수 있슴
    public void CheckValidCreateRoom()
    {
        if (m_vRoomNameInputField.text != "" && m_vMaxPlayerInputField.text != "" && (m_vAutoRoleToggle.isOn || m_vNumberOfMafiaInputField.text != ""))
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
