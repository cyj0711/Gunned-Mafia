using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreBoardManager : Singleton<ScoreBoardManager>
{

    [SerializeField] private GameObject m_vScoreBoardPanelObject;
    [SerializeField] private Scrollbar m_vScoreBoardScrollBar;
    [SerializeField] private GameObject m_vScoreBoardItemPrefab;

    [SerializeField] private Transform m_vAlivePlayerListHeader;
    [SerializeField] private Transform m_vMissingPlayerListHeader;
    [SerializeField] private Transform m_vDeadPlayerListHeader;
    [SerializeField] private Transform m_vSpectatorPlayerListHeader;

    [SerializeField] private Transform m_vAlivePlayerListTransform;
    [SerializeField] private Transform m_vMissingPlayerListTransform;
    [SerializeField] private Transform m_vDeadPlayerListTransform;
    [SerializeField] private Transform m_vSpectatorPlayerListTransform;

    private Dictionary<int, ScoreBoardItemController> m_dicScoreBoardItems = new Dictionary<int, ScoreBoardItemController>();

    private Dictionary<int, ScoreBoardItemController> m_dicMissingPlayerItems = new Dictionary<int, ScoreBoardItemController>();

    public void ShowScoreBoard(bool bActive)
    {
        m_vScoreBoardPanelObject.SetActive(bActive);
    }

    public void CreateScoreBoardItem(int _iActorNumber, ScoreBoardItemController _vScoreBoardItem)
    {
        if (!m_dicScoreBoardItems.ContainsKey(_iActorNumber))
        {
            m_dicScoreBoardItems.Add(_iActorNumber, _vScoreBoardItem);
        }
        else
        {
            Debug.LogError("Tried to Add ScoreBoard, but player " + _iActorNumber + " alredy exist");
            return;
        }
    }

    public void RemoveScoreBoardItem(int _iActorNumber)
    {
        if (m_dicScoreBoardItems.ContainsKey(_iActorNumber))
        {
            ScoreBoardItemController vScoreBoardItemController = m_dicScoreBoardItems[_iActorNumber];
            m_dicScoreBoardItems.Remove(_iActorNumber);
            Destroy(vScoreBoardItemController.gameObject);
        }
    }

    public void SetScoreBoardItemParent(int _iActorNumber, E_PlayerState _ePlayerState)
    {
        switch (_ePlayerState)
        {
            case E_PlayerState.Alive:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vAlivePlayerListTransform);

                if (m_dicMissingPlayerItems.ContainsKey(_iActorNumber))
                    m_dicMissingPlayerItems.Remove(_iActorNumber);

                break;
            case E_PlayerState.Missing: // 마피아와 사망자(관전자)만 현재 실종자가 누구인지 알 수 있다.
                //m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vMissingPlayerListTransform);
                //if (GameManager.I.GetPlayerRole() == E_PlayerRole.Civil || GameManager.I.GetPlayerRole() == E_PlayerRole.Detective)
                //{
                //    if (GameManager.I.GetPlayerController().a_ePlayerState == E_PlayerState.Alive)
                //        m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vAlivePlayerListTransform);
                //}
                if ((GameManager.I.GetPlayerController().a_ePlayerState != E_PlayerState.Alive) || (GameManager.I.GetPlayerRole() == E_PlayerRole.Mafia))
                    m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vMissingPlayerListTransform);

                m_dicMissingPlayerItems.Add(_iActorNumber, m_dicScoreBoardItems[_iActorNumber]);

                break;
            case E_PlayerState.Dead:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vDeadPlayerListTransform);

                if (m_dicMissingPlayerItems.ContainsKey(_iActorNumber))
                    m_dicMissingPlayerItems.Remove(_iActorNumber);

                break;
            case E_PlayerState.Spectator:
                m_dicScoreBoardItems[_iActorNumber].transform.SetParent(m_vSpectatorPlayerListTransform);

                if (m_dicMissingPlayerItems.ContainsKey(_iActorNumber))
                    m_dicMissingPlayerItems.Remove(_iActorNumber);

                break;
        }
        m_dicScoreBoardItems[_iActorNumber].transform.localScale = new Vector3(1f, 1f, 1f);
        HideHeaderWithNoItem();
    }

    private void HideHeaderWithNoItem()
    {
        if (m_vAlivePlayerListTransform.childCount > 0)
            m_vAlivePlayerListHeader.gameObject.SetActive(true);
        else
            m_vAlivePlayerListHeader.gameObject.SetActive(false);

        if (m_vMissingPlayerListTransform.childCount > 0)
            m_vMissingPlayerListHeader.gameObject.SetActive(true);
        else
            m_vMissingPlayerListHeader.gameObject.SetActive(false);

        if (m_vDeadPlayerListTransform.childCount > 0)
            m_vDeadPlayerListHeader.gameObject.SetActive(true);
        else
            m_vDeadPlayerListHeader.gameObject.SetActive(false);

        if (m_vSpectatorPlayerListTransform.childCount > 0)
            m_vSpectatorPlayerListHeader.gameObject.SetActive(true);
        else
            m_vSpectatorPlayerListHeader.gameObject.SetActive(false);
    }

    public void UpdateAllPlayerScoreBoard()
    {
        foreach (KeyValuePair<int, ScoreBoardItemController> _dicScoreBoardItem in m_dicScoreBoardItems)
        {
            ScoreBoardItemController _vPlayerScoreBoard = _dicScoreBoardItem.Value;

            _vPlayerScoreBoard.UpdatePlayerRealRole();
        }

        foreach (KeyValuePair<int, ScoreBoardItemController> _dicScoreBoardItem in m_dicScoreBoardItems)
        {
            ScoreBoardItemController _vPlayerScoreBoard = _dicScoreBoardItem.Value;

            _vPlayerScoreBoard.UpdatePlayerRealRole();
        }
    }
}
