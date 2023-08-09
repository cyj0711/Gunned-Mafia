using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameManager : Singleton<UIGameManager>
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
    [SerializeField] private Button m_vTrackDnaButton;
    [SerializeField] private Button m_vCallDetectiveButton;

    [SerializeField] private GameObject m_vNotificationItemPrefab;
    [SerializeField] private Transform m_vNotificationContainerTransform;

    bool m_bIsStartMessageSend = false; // 시작 알림 반복 호출 방지

    float m_fNotificationYMoveSize; // 알림 메시지가 새로 생길때마다 기존 메세지가 밑으로 가는 정도(알림메세지 프리팹의 height + vertical layout group의 spacing 의 합임
    Vector3 m_vOriginPosition;

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

        if (ePlayerRole == E_PlayerRole.Detective)
        {
            m_vTrackDnaButton.gameObject.SetActive(true);
        }
        else
        {
            m_vTrackDnaButton.gameObject.SetActive(false);
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
}
