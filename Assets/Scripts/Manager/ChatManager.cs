using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum E_ChatType
{
    Chat,   // 일반 유저들 끼리의 채팅
    System, // 시스템 상 알림 (입장, 퇴장 등)
    Game    // 게임 상 알림 (게임 시작, 사망, 게임 끝 등)
}

public class ChatManager : Singleton<ChatManager>
{
    [SerializeField] private PhotonView m_vPhotonView;

    [SerializeField] private InputField m_vInputField;
    public InputField a_vInputField { get => m_vInputField; }
    [SerializeField] private Text m_vChatText;
    [SerializeField] private ScrollRect m_vScrollRect;
    [SerializeField] private Scrollbar m_vScrollBar;
    [SerializeField] private Image m_vChatScrollViewImage;
    [SerializeField] private Image m_vInputFieldImage;
    [SerializeField] private TMP_Text m_vTargetToChatText;
    [SerializeField] private Button m_vTargetToChatButton;

    private bool m_bIsFocusedPast = false; // 이전 프레임에서 inputField가 포커스를 가졌는지 여부를 저장하는 변수
    private bool m_bIsTeamChat = false;

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
        else if(Input.GetKeyDown(KeyCode.Tab))
        {
            if(m_vInputField.isFocused)
            {
                ToggleTeamChat();
            }
        }

        DisplayChatPanel();
    }

    // 채팅 치는 도중에는 채팅 패널을 활성화한다.
    public void DisplayChatPanel()
    {
        if (m_bIsFocusedPast == m_vInputField.isFocused) return;

        // 채팅창 활성화
        if (m_vInputField.isFocused)
        {
            m_vChatScrollViewImage.color = new Color(1f, 1f, 1f, 0.4f);
            m_vScrollRect.verticalScrollbar = m_vScrollBar;

            m_vTargetToChatButton.gameObject.SetActive(true);
        }
        // 채팅창 비활성화
        else
        {
            m_vInputField.interactable = false;
            m_vChatScrollViewImage.color = new Color(1f, 1f, 1f, 0f);

            m_vScrollRect.verticalScrollbar = null;
            m_vScrollBar.gameObject.SetActive(false);

            m_vTargetToChatButton.gameObject.SetActive(false);
        }

        m_bIsFocusedPast = m_vInputField.isFocused;
    }

    // Tab을 누르면 전체채팅과 팀채팅을 바꾼다
    private void ToggleTeamChat()
    {
        // 팀채팅은 살아있는 마피아 플레이어만 사용할 수 있다.
        if ((GameManager.I.GetPlayerRole() != E_PlayerRole.Mafia) || (GameManager.I.GetPlayerController().a_ePlayerState != E_PlayerState.Alive))
            return;

        ToggleTeamChat(!m_bIsTeamChat);
    }

    // 시스템 상에서 전체채팅과 팀채팅을 강제로 바꾼다.
    public void ToggleTeamChat(bool _bIsTeamChat)
    {
        m_bIsTeamChat = _bIsTeamChat;

        m_vTargetToChatText.text = m_bIsTeamChat ? "Team" : "All";
        m_vTargetToChatText.color = m_bIsTeamChat ? UIColor.TextRed : UIColor.Black;
    }

    // Activate by Chat Input -> On End Edit
    public void UserChatInput(string _strChat)
    {
        m_vInputField.text = "";
        if (_strChat == "")
            return;

        SendChat(E_ChatType.Chat, _strChat, m_bIsTeamChat);
    }

    public void SendChat(E_ChatType _eChatType, string _strChat, bool _bIsTeamChat = false)
    {
        m_vPhotonView.RPC(nameof(ChatRPC), RpcTarget.AllViaServer, (int)_eChatType, PhotonNetwork.LocalPlayer.ActorNumber, _strChat, _bIsTeamChat);
    }

    [PunRPC]
    private void ChatRPC(int _iChatType, int _iSenderActorNumber, string _strChatMessage, bool _bIsTeamChat)
    {
        switch ((E_ChatType)_iChatType)
        {
            case E_ChatType.Chat:
                // 사망자는 사망자끼리만 대화가 가능함
                if (GameManager.I.GetPlayerController(_iSenderActorNumber).a_ePlayerState != E_PlayerState.Alive)
                {
                    if (GameManager.I.GetPlayerController().a_ePlayerState != E_PlayerState.Alive)
                    {
                        m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextGray) + ">" + "(dead) " + "</color>" + 
                            "<color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextYellow) + ">" + GameManager.I.GetPlayerNickName(_iSenderActorNumber) + "</color></b>" + ": " + _strChatMessage;
                    }
                    else
                    {
                        return;
                    }
                }
                // 팀 채팅은 마피아끼리만 대화할 수 있다.
                else if (_bIsTeamChat)
                {
                    if (GameManager.I.GetPlayerRole() == E_PlayerRole.Mafia)
                    {
                        m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextGray) + ">" + "(team) " + "</color>" +
                            "<color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextRed) + ">" + GameManager.I.GetPlayerNickName(_iSenderActorNumber) + "</color></b>" + ": " + _strChatMessage;
                    }
                    else
                    {
                        return;
                    }
                }
                // 살아있는 플레이어들의 전체 채팅
                else
                {
                    m_vChatText.text += "\r\n" + "<b><color=#" + (GameManager.I.GetPlayerRole(_iSenderActorNumber) == E_PlayerRole.Detective ? ColorUtility.ToHtmlStringRGB(UIColor.TextBlue) : ColorUtility.ToHtmlStringRGB(UIColor.TextGreen)) + ">"
                        + GameManager.I.GetPlayerNickName(_iSenderActorNumber) + "</color></b>" + ": " + _strChatMessage;
                }
                break;
            case E_ChatType.Game:
                m_vChatText.text += "\r\n" + _strChatMessage;
                break;
            case E_ChatType.System:
                m_vChatText.text += "\r\n" + "<b><color=#" + ColorUtility.ToHtmlStringRGB(UIColor.TextSkyBlue) + ">" + _strChatMessage + "</color></b>";
                break;
        }

        // 메세지 크기가 일정 수를 넘어가면 오래된 메세지부터 없앤다.
        while (m_vChatText.text.Length > 4000)
        {
            RemoveOldChat();
        }

        // 플레이어가 채팅 스크롤바를 올려서 과거 채팅을 보고있을 땐 새 채팅이 와도 채팅창을 아래로 내리지 않는다.
        if (m_vScrollBar.value <= 0 || PhotonNetwork.LocalPlayer.ActorNumber== _iSenderActorNumber || !m_vInputField.isFocused)
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

    public void RemoveOldChat()
    {
        int index = m_vChatText.text.IndexOf('\n');

        if (index != -1)
        {
            m_vChatText.text = m_vChatText.text.Substring(index + 1);
        }
    }
}
