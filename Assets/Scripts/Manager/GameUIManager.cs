using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : Singleton<GameUIManager>
{
    [SerializeField] private PhotonView m_vPhotonView;

    [SerializeField] private Text m_vTimeText;
    [SerializeField] private Text m_vStatusText;
    [SerializeField] private Image m_vStatusImage;

    [SerializeField] private GameObject m_vAmmoArea;
    [SerializeField] private Text m_vAmmoText;
    [SerializeField] private Image m_vAmmoImage;

    [SerializeField] private GameObject m_vSearchPanelObject;
    [SerializeField] private Text m_vSearchText;

    //[SerializeField] private GameObject m_vScoreBoardPanelObject;
    //[SerializeField] private Scrollbar m_vScoreBoardScrollBar;
    //[SerializeField] private GameObject m_vScoreBoardItemPrefab;

    //[SerializeField] private Transform m_vAlivePlayerListHeader;
    //[SerializeField] private Transform m_vMissingPlayerListHeader;
    //[SerializeField] private Transform m_vDeadPlayerListHeader;
    //[SerializeField] private Transform m_vSpectatorPlayerListHeader;

    //[SerializeField] private Transform m_vAlivePlayerListTransform;
    //[SerializeField] private Transform m_vMissingPlayerListTransform;
    //[SerializeField] private Transform m_vDeadPlayerListTransform;
    //[SerializeField] private Transform m_vSpectatorPlayerListTransform;

    //private Dictionary<int, ScoreBoardItemController> m_dicScoreBoardItems = new Dictionary<int, ScoreBoardItemController>();

    [SerializeField] private GameObject m_vNotificationItemPrefab;
    [SerializeField] private Transform m_vNotificationContainerTransform;

    bool m_bIsStartMessageSend = false; // 시작 알림 반복 호출 방지

    void Start()
    {
        SetGameState();
        SetTimeText();
    }

    void Update()
    {
        //SetGameState();
        SetTimeText();
    }

    public void SetGameState()
    {
        E_GAMESTATE eGameState = GameManager.I.a_eGameState;

        switch (eGameState)
        {
            case E_GAMESTATE.Wait:
                m_vStatusImage.color = UIColor.Gray;
                m_vStatusText.text = eGameState.ToString();
                m_vTimeText.enabled = false;
                break;
            case E_GAMESTATE.Play:
                SetRole();
                m_vTimeText.enabled = true;
                break;
            default:
                m_vStatusImage.color = UIColor.Gray;
                m_vStatusText.text = eGameState.ToString();
                m_vTimeText.enabled = true;
                break;
        }
    }

    private void SetRole()
    {
        E_PlayerRole ePlayerRole = GameManager.I.GetPlayerRole();

        switch (ePlayerRole)
        {
            case E_PlayerRole.Civil:
                m_vStatusImage.color = UIColor.Green;
                break;
            case E_PlayerRole.Mafia:
                m_vStatusImage.color = UIColor.Red;
                break;
            case E_PlayerRole.Detective:
                m_vStatusImage.color = UIColor.Blue;
                break;
            default:
                m_vStatusImage.color = UIColor.Gray;
                break;
        }
        m_vStatusText.text = ePlayerRole.ToString();

        if (!m_bIsStartMessageSend)
        {
            CreateNotification("The Game is Begined !!");
            CreateNotification("You are " + ePlayerRole);

            m_bIsStartMessageSend = true;
        }
    }

    private void SetTimeText()
    {
        int iTime = (int)GameManager.I.GetTime();
        m_vTimeText.text = (iTime / 60).ToString("D2") + ":" + (iTime % 60).ToString("D2");
    }

    public void SetAmmo(int _iAmmoCapacity, int _iCurrentAmmo, int _iRemainAmmo)
    {
        m_vAmmoImage.fillAmount = (float)(_iCurrentAmmo / (float)_iAmmoCapacity);
        m_vAmmoText.text = _iCurrentAmmo.ToString("D2") + " / " + _iRemainAmmo.ToString("D2");
    }

    public void SetAmmoActive(bool _bIsActive)
    {
        m_vAmmoArea.SetActive(_bIsActive);
    }

    public void SetSearchPanelActive(bool _bIsActive)
    {
        m_vSearchPanelObject.SetActive(_bIsActive);
    }

    public void SetSearchText(int _iVictim, E_PlayerRole _eVictimRole, int _iWeapon, int _iDeadTime)
    {
        // string sText = "This is the body of ''. His role is ''! He was killed by a ''. It's been '' seconds since he died.";
        string sText =
            "This is the body of " + PhotonNetwork.CurrentRoom.GetPlayer(_iVictim).NickName +
            ". His role is " + _eVictimRole +
            ". He was killed by a " + DataManager.I.GetWeaponDataWithID(_iWeapon).a_strWeaponName +
            ". It's been " + _iDeadTime / 60 + " minutes and " + _iDeadTime % 60 + " seconds since he died.";

        m_vSearchText.text = sText;
    }

    // 로컬 플레이어에게 알림 메세지 전송
    public void CreateNotification(string _strText)
    {
        NotificationItemController vNotificationItem = Instantiate(m_vNotificationItemPrefab).GetComponent<NotificationItemController>();
        vNotificationItem.SetText(_strText);

        vNotificationItem.transform.SetParent(m_vNotificationContainerTransform);
        vNotificationItem.transform.SetAsFirstSibling();    // 새 알림창은 항상 위에서부터 표시된다.
        vNotificationItem.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    // 모든 플레이어에게 알림 메세지 전송
    public void CreateNotificationToAll(string _strText)
    {
        m_vPhotonView.RPC(nameof(CreateNotificationToAllRPC), RpcTarget.AllViaServer, _strText);
    }

    [PunRPC]
    private void CreateNotificationToAllRPC(string _strText)
    {
        CreateNotification(_strText);
    }
}
