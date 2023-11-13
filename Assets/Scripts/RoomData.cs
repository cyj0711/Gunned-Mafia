using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoomData : MonoBehaviour
{
    [SerializeField] TMP_Text m_vRoomInfoText;
    [SerializeField] RoomInfo m_vRoomInfo;
    public RoomInfo a_vRoomInfo {get{ return m_vRoomInfo; } }
    public void SetRoomInfo(RoomInfo _value)
    {
        m_vRoomInfo = _value;
        m_vRoomInfoText.text = $"{m_vRoomInfo.Name} ({m_vRoomInfo.PlayerCount}/{m_vRoomInfo.MaxPlayers})";
    }

    public void OnClickJoinRoom()
    {
        if(LobbyManager.I.a_vNickNameInputField.text=="")
        {
            LobbyManager.I.SetActiveWarningMessage(true, "Please Enter Your NicknameFirst!!!");
            return;
        }

        if (m_vRoomInfo == null) return;

        if ((string)m_vRoomInfo.CustomProperties["Password"] == "")
        {
            JoinRoom();
        }
        else
        {
            LobbyManager.I.SetActiveInputPasswordPanel(this);
        }
    }

    public void JoinRoom()
    {
        if (m_vRoomInfo == null) return;

        PhotonNetwork.JoinRoom(m_vRoomInfo.Name);
    }
}
