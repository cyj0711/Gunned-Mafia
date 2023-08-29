using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum E_ChatType
{
    Chat,   // 일반 유저들 끼리의 채팅
    System, // 시스템 상 알림 (입장, 퇴장 등)
    Game    // 게임 상 알림 (게임 시작, 사망, 게임 끝 등)
}

public class ChatManagerWithoutPhotonChat : Singleton<ChatManagerWithoutPhotonChat>
{
    [SerializeField] private PhotonView m_vPhotonView;

    [SerializeField] private InputField m_vInputField;
    public InputField a_vInputField { get => m_vInputField; }
    [SerializeField] private Text m_vChatText;
    [SerializeField] private ScrollRect m_vScrollRect;
    [SerializeField] private Scrollbar m_vScrollBar;
    [SerializeField] private Image m_vChatScrollViewImage;
    [SerializeField] private Image m_vInputFieldImage;

    private bool m_bIsFocusedPast = false; // 이전 프레임에서 inputField가 포커스를 가졌는지 여부를 저장하는 변수

    void Start()
    {
        SendChat(E_ChatType.System, PhotonNetwork.LocalPlayer.NickName + " joined the game.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            m_vInputField.interactable = true;
            m_vInputField.Select();
        }

        DisplayChatPanel();
    }

    // 채팅 치는 도중에는 채팅 패널을 활성화한다.
    public void DisplayChatPanel()
    {
        if (m_bIsFocusedPast == m_vInputField.isFocused) return;

        if (m_vInputField.isFocused)
        {
            m_vChatScrollViewImage.color = new Color(1f, 1f, 1f, 0.4f);
            m_vScrollRect.verticalScrollbar = m_vScrollBar;
        }
        else
        {
            m_vInputField.interactable = false;
            m_vChatScrollViewImage.color = new Color(1f, 1f, 1f, 0f);

            m_vScrollRect.verticalScrollbar = null;
            m_vScrollBar.gameObject.SetActive(false);
        }

        m_bIsFocusedPast = m_vInputField.isFocused;
    }

    // Activate by Chat Input -> On End Edit
    public void UserChatInput(string _strChat)
    {
        m_vInputField.text = "";
        if (_strChat == "")
            return;

        SendChat(E_ChatType.Chat, _strChat);
    }    

    public void SendChat(E_ChatType _eChatType, string _strChat)
    {
        m_vPhotonView.RPC(nameof(ChatRPC), RpcTarget.AllViaServer, (int)_eChatType, PhotonNetwork.LocalPlayer.ActorNumber, _strChat);
    }

    [PunRPC]
    private void ChatRPC(int _iChatType, int _iSenderActorNumber, string _strChatMessage)
    {
        switch ((E_ChatType)_iChatType)
        {
            case E_ChatType.Chat:
                // 사망자는 사망자끼리만 대화가 가능함
                if (GameManager.I.GetPlayerController(_iSenderActorNumber).a_ePlayerState != E_PlayerState.Alive)
                {
                    if (GameManager.I.GetPlayerController().a_ePlayerState != E_PlayerState.Alive)
                    {
                        m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextGray) + ">" + "(dead) " + "</color>"+"<color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextYellow) + ">" + GameManager.I.GetPlayerNickName(_iSenderActorNumber) + "</color></b>" + ": " + _strChatMessage;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextGreen) + ">" + GameManager.I.GetPlayerNickName(_iSenderActorNumber) + "</color></b>" + ": " + _strChatMessage;
                }
                break;
            case E_ChatType.Game:
                m_vChatText.text += "\r\n" + _strChatMessage;
                break;
            case E_ChatType.System:
                m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextBlue) + ">" + _strChatMessage + "</color></b>";
                break;
        }

        // 플레이어가 채팅 스크롤바를 올려서 과거 채팅을 보고있을 땐 새 채팅이 와도 채팅창을 아래로 내리지 않는다.
        if (m_vScrollBar.value <= 0 || PhotonNetwork.LocalPlayer.ActorNumber== _iSenderActorNumber)
        { 
            // 새 채팅이 올때마다 채팅창을 제일 아래로 갱신한다.
            StartCoroutine(nameof(ScrollToBottom));
        }
    }

    // UGUI의 스케줄러 문제로 한 프레임 뒤에 Scroll Rect를 계산해야하므로 Coroutine으로 프레임을 기다린다.
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        m_vScrollRect.verticalNormalizedPosition = 0.0f;
    }
}
