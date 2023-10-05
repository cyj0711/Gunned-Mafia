using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuManager : Singleton<UIMenuManager>
{
    [SerializeField] GameObject m_vMenuPanelObject;
    [SerializeField] GameObject m_vRoomSettingPanelObject;

    [SerializeField] Button m_vRoomSettingButton;

    [SerializeField] TMP_InputField m_vNumberOfMafiaInputField;
    [SerializeField] TMP_InputField m_vNumberOfDetectiveInputField;

    [SerializeField] Toggle m_vAutoRoleToggle;

    bool m_bISMenuActive = false;

    private void Start()
    {
        if(PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            m_vRoomSettingButton.interactable = true;
        }
        else
        {
            m_vRoomSettingButton.interactable = false;
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            m_bISMenuActive = !m_bISMenuActive;

            m_vMenuPanelObject.SetActive(m_bISMenuActive);
        }
    }

    public void OnClickRoomSettingButton()
    {
        ((TMP_Text)m_vNumberOfMafiaInputField.placeholder).text = "1 ~ " + (PhotonNetwork.CurrentRoom.MaxPlayers - 1);
        ((TMP_Text)m_vNumberOfDetectiveInputField.placeholder).text = "0 ~ " + (PhotonNetwork.CurrentRoom.MaxPlayers - 1);

        m_vAutoRoleToggle.isOn = (bool)PhotonNetwork.CurrentRoom.CustomProperties["IsAutoRole"];

        if((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfMafia"]==-1)
        {
            m_vNumberOfMafiaInputField.text = "";
        }
        else
        {
            m_vNumberOfMafiaInputField.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfMafia"]).ToString();
        }

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfDetective"] == -1)
        {
            m_vNumberOfDetectiveInputField.text = "";
        }
        else
        {
            m_vNumberOfDetectiveInputField.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfDetective"]).ToString();
        }

        m_vNumberOfMafiaInputField.interactable = !m_vAutoRoleToggle.isOn;
        m_vNumberOfDetectiveInputField.interactable = !m_vAutoRoleToggle.isOn;

        m_vRoomSettingPanelObject.SetActive(true);
    }

    public void EndEditMafiaInputField(string _strInput)
    {
        if (_strInput == "") return;

        m_vNumberOfMafiaInputField.text = Mathf.Clamp(int.Parse(_strInput), 1, PhotonNetwork.CurrentRoom.MaxPlayers - 1).ToString();

        // 만약 마피아 수 + 탐정 수가 최대 인원을 넘어서면 넘어선만큼 탐정 수를 조정한다.
        if (m_vNumberOfDetectiveInputField.text != "")
        {
            if (int.Parse(m_vNumberOfMafiaInputField.text) + int.Parse(m_vNumberOfDetectiveInputField.text) > PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                m_vNumberOfDetectiveInputField.text = (PhotonNetwork.CurrentRoom.MaxPlayers - int.Parse(m_vNumberOfMafiaInputField.text)).ToString();
            }
        }
    }

    public void EndEditDetectiveInputField(string _strInput)
    {
        if (_strInput == "") return;

        m_vNumberOfDetectiveInputField.text = Mathf.Clamp(int.Parse(_strInput), 0, PhotonNetwork.CurrentRoom.MaxPlayers - 1).ToString();

        // 만약 탐정 수 + 마피아 수가 최대 인원을 넘어서면 넘어선만큼 마피아 수를 조정한다.
        if (m_vNumberOfMafiaInputField.text != "")
        {
            if (int.Parse(m_vNumberOfDetectiveInputField.text) + int.Parse(m_vNumberOfMafiaInputField.text) > PhotonNetwork.CurrentRoom.MaxPlayers)
            {
                m_vNumberOfMafiaInputField.text = (PhotonNetwork.CurrentRoom.MaxPlayers - int.Parse(m_vNumberOfDetectiveInputField.text)).ToString();
            }
        }
    }

    public void OnValueChangedAutoRoleSetting(bool _bValue)
    {
        m_vNumberOfMafiaInputField.interactable = !_bValue;
        m_vNumberOfDetectiveInputField.interactable = !_bValue;

        //m_vNumberOfMafiaInputField.text = "";
        //m_vNumberOfDetectiveInputField.text = "";
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfMafia"] != -1)
            m_vNumberOfMafiaInputField.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfMafia"]).ToString();
        else
            m_vNumberOfMafiaInputField.text = "";

        if ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfDetective"] != -1)
            m_vNumberOfDetectiveInputField.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties["NumberOfDetective"]).ToString();
        else
            m_vNumberOfDetectiveInputField.text = "";
    }

    public void OnClickRoomSettingApplyButton()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameManager.I.SetRoleCustomProperty(m_vAutoRoleToggle.isOn, int.Parse(m_vNumberOfMafiaInputField.text), int.Parse(m_vNumberOfDetectiveInputField.text));
            ChatManager.I.SendChat(E_ChatType.System, "The host has changed the settings for the game. This will be applied in the next round.");
        }
        m_vRoomSettingPanelObject.SetActive(false);
    }

    public void OnClickRoomSettingCancelButton()
    {
        m_vRoomSettingPanelObject.SetActive(false);
    }

    // 새 마스터 클라이언트로 바뀌었을때 해당 마스터 클라이언트에서 호출하는 함수
    public void MasterClientSwitchedProcess()
    {
        m_vRoomSettingButton.interactable = true;
    }
}
