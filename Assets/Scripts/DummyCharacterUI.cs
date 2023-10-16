using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DummyCharacterUI : MonoBehaviour
{
    int m_iHealth;
    public int a_iHealth { set { m_iHealth = value; SetHealthUI(); } }

    int m_iPlayerActorNumber;
    public int a_iPlayerActorNumber { set { m_iPlayerActorNumber = value; } }

    [SerializeField]
    Text m_vNickNameText;
    [SerializeField]
    Text m_vHealthText;
    [SerializeField]
    GameObject m_vCanvasBody;

    private void SetHealthUI()
    {
        m_vHealthText.text = m_iHealth.ToString();
    }

    //private void OnMouseEnter()
    //{
    //    Vector3 vLocalPlayerPosition = GameManager.I.GetPlayerController().transform.position;
    //    RaycastHit2D hit = Physics2D.Raycast(transform.position, vLocalPlayerPosition - transform.position, Vector2.Distance(vLocalPlayerPosition, transform.position), 1 << LayerMask.NameToLayer("BlockAll"));
    //    if (hit.transform == null)
    //    {
    //        m_vCanvasBody.SetActive(true);
    //    }

    //}

    private void OnMouseOver()
    {
        Vector3 vLocalPlayerPosition = GameManager.I.GetPlayerController().transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, vLocalPlayerPosition - transform.position, Vector2.Distance(vLocalPlayerPosition, transform.position), 1 << LayerMask.NameToLayer("BlockAll"));
        
        if (hit.transform != null)
        {
            m_vCanvasBody.SetActive(false);
        }
        else
        {
            m_vCanvasBody.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).a_ePlayerState == E_PlayerState.Alive)
            m_vCanvasBody.SetActive(false);

    }

    public void SetCanvasBodyActive(bool _bIsActive)
    {
        m_vCanvasBody.SetActive(_bIsActive);
    }
}
