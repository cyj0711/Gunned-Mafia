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

    float m_fNotificationYMoveSize; // 알림 메시지가 새로 생길때마다 기존 메세지가 밑으로 가는 정도(알림메세지 프리팹의 height + vertical layout group의 spacing 의 합임
    Vector3 m_vOriginPosition;

    int m_iVictimActorNumber = -1;   // 시체 탐색 ui에서 해당 시체 주인이 누구인지 확인(call detective 버튼으로 탐정에게 위치를 보내는 용도)

    void Start()
    {
        SetGameState();
        SetTimeText();

        m_fNotificationYMoveSize = m_vNotificationItemPrefab.GetComponent<RectTransform>().sizeDelta.y + m_vNotificationContainerTransform.gameObject.GetComponent<VerticalLayoutGroup>().spacing;
        m_vOriginPosition = m_vNotificationContainerTransform.localPosition;
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

    public void SetSearchText(int _iVictim,string _strVictimNickName, E_PlayerRole _eVictimRole, int _iWeapon, int _iDeadTime)
    {
        // string sText = "This is the body of ''. His role is ''! He was killed by a ''. It's been '' seconds since he died.";
        string sText =
            "This is the body of " + _strVictimNickName +
            ". His role is " + _eVictimRole +
            ". He was killed by a " + DataManager.I.GetWeaponDataWithID(_iWeapon).a_strWeaponName +
            ". It's been " + _iDeadTime / 60 + " minutes and " + _iDeadTime % 60 + " seconds since he died.";

        m_vSearchText.text = sText;
        m_iVictimActorNumber = _iVictim;
    }

    public void CreateNotification(string _strText)
    {
        StartCoroutine(nameof(NotificationSmoothMoveCoroutine), _strText);
    }

    private IEnumerator NotificationSmoothMoveCoroutine(string _strText)
    {
        float elapsedTime = 0.0f;
        float fMoveTime = 0.2f;

        Vector3 vTargetPosition = m_vOriginPosition + new Vector3(0f, -m_fNotificationYMoveSize, 0f);

        /* 전체 메세지들을 아래로 살짝 내리는 단계 */

        while (elapsedTime < fMoveTime)
        {
            m_vNotificationContainerTransform.localPosition = Vector3.Lerp(m_vOriginPosition, vTargetPosition, elapsedTime / fMoveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        /* 새 알림 메세지 생성 단계 */

        m_vNotificationContainerTransform.localPosition = m_vOriginPosition;

        NotificationItemController vNotificationItem = Instantiate(m_vNotificationItemPrefab).GetComponent<NotificationItemController>();
        vNotificationItem.SetText(_strText);

        vNotificationItem.transform.SetParent(m_vNotificationContainerTransform);
        vNotificationItem.transform.SetAsFirstSibling();    // 새 알림 메세지는 항상 위에서부터 표시된다.
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

    // 시체 조사 창에서 call detective 버튼을 누르면 해당 함수를 호출하여 탐정 플레이어들에게 시체의 위치를 알려준다.
    public void CallDetective()
    {
        List<int>listDetectivePlayers = GameManager.I.GetDetectivePlayers();

        PlayerDeadController vDeadPlayer = MapManager.I.GetPlayerDead(m_iVictimActorNumber);

        if (vDeadPlayer == null)
            return;

        foreach (int indexDetectivePlayers in listDetectivePlayers)
        {
            DisplayLocation(indexDetectivePlayers, vDeadPlayer.transform.position, 30f);
        }
    }

    // _iPlayerActorNumber 플레이어의 화면에 _vTargetPosition 의 위치를 _fDisplayTime 동안 표시한다.
    public void DisplayLocation(int _iPlayerActorNumber, Vector3 _vTargetPosition, float _fDisplayTime)
    {
        m_vPhotonView.RPC(nameof(DisplayLocationRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iPlayerActorNumber), _vTargetPosition, _fDisplayTime);
    }

    [PunRPC]
    private void DisplayLocationRPC(Vector3 _vTargetPosition, float _fDisplayTime)
    {
        GameObject vLocationPingObject = Instantiate(DataManager.I.a_vLocationPingPrefab);
        LocationPingController vLocationPingController = vLocationPingObject.GetComponent<LocationPingController>();

        if(vLocationPingObject==null)
        {
            Destroy(vLocationPingObject);
            return;
        }
        vLocationPingObject.transform.position = _vTargetPosition;

        vLocationPingController.InitData(GameManager.I.GetPlayerController().transform, _fDisplayTime);
    }
}
