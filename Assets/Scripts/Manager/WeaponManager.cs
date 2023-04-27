using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks , IPunObservable
{
    [SerializeField]
    private PhotonView m_vPhotonView;

    private Dictionary<E_WeaponType, WeaponBase> m_dicWeaponInventory = new Dictionary<E_WeaponType, WeaponBase>();

    private WeaponBase m_vCurrentWeapon;

    private bool m_bSeeingRight = true;

    //*************** Synchronization Properties *******************
    [SerializeField] int m_iCurrentWeaponViewID;   // 아무것도 안들었을땐 -1 로 설정해주자.
    public int a_iCurrentWeaponViewID { get => m_iCurrentWeaponViewID; set => SetPropertyRPC(nameof(CurrentWeaponIDRPC), value); }
    [PunRPC] void CurrentWeaponIDRPC(int value)
    { 
        m_iCurrentWeaponViewID = value;

        if (a_iCurrentWeaponViewID == -1) { return; }

        GameObject vWeaponObject = PhotonView.Find(m_iCurrentWeaponViewID).gameObject;
        if (m_vCurrentWeapon != null)
        {
            m_vCurrentWeapon.gameObject.SetActive(false);
        }

        m_vCurrentWeapon = vWeaponObject.GetComponent<WeaponBase>();
        if (m_vCurrentWeapon == null)
        {
            Debug.Log("Debug ERROR : current " + vWeaponObject + " weaponBase is NULL");
            return;
        }

        SetDirection(m_bSeeingRight);
        vWeaponObject.SetActive(true);
    }
    //**************************************************************

    void SetPropertyRPC(string functionName, object value)
    {
        m_vPhotonView.RPC(functionName, RpcTarget.All, value);
    }

    public void InvokeProperties()  // Synchronization properites에 새 속성을 넣을경우 여기에 반드시 추가한다.(변수명이 아닌 get set 명임!!)
    {
        a_iCurrentWeaponViewID = a_iCurrentWeaponViewID;
    }

    void Start()
    {
        //InitWeaponManager();
    }

    public void InitWeaponManager()
    {
        m_vCurrentWeapon = null;
        m_dicWeaponInventory.Clear();
    }

    // 플레이어가 마우스를 누르면 이 함수가 호출되어 총알 발사
    public void Shoot(float angle)
    {
        if(m_vCurrentWeapon!=null)
        {
            m_vCurrentWeapon.Shoot(transform.rotation.eulerAngles.z, PhotonNetwork.LocalPlayer.ActorNumber);
        }
    }

    public void Reload()
    {
        if (m_vCurrentWeapon != null)
        {
            m_vCurrentWeapon.Reload();
        }
    }

    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    public void SetDirection(bool playerSeeingRight)
    {
        m_bSeeingRight = playerSeeingRight;
        if (m_vCurrentWeapon != null)
        {
            //m_vCurrentWeapon.gameObject.GetComponent<SpriteRenderer>().flipY = !m_bSeeingRight;
            if(playerSeeingRight)
            {
                if (m_vCurrentWeapon.gameObject.transform.localScale.y < 0)
                    m_vCurrentWeapon.gameObject.transform.localScale = new Vector3
                        (m_vCurrentWeapon.gameObject.transform.localScale.x, m_vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);
            }
            else
            {
                if (m_vCurrentWeapon.gameObject.transform.localScale.y > 0)
                    m_vCurrentWeapon.gameObject.transform.localScale = new Vector3
                        (m_vCurrentWeapon.gameObject.transform.localScale.x, m_vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);
            }
        }

    }

    // 숫자키를 입력받으면 그에 해당하는 무기로 변경
    public void ChangeCurrentWeapon(int iInputKeyNumber)
    {
        switch(iInputKeyNumber)
        {
            case 1:
                if(CheckCanEquipWeaponType(E_WeaponType.Primary))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Primary].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmo();
                    GamePanelManager.I.SetAmmoActive(true);
                }
                break;
            case 2:
                if (CheckCanEquipWeaponType(E_WeaponType.Secondary))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Secondary].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmo();
                    GamePanelManager.I.SetAmmoActive(true);
                }
                break;
            case 3:
                if (CheckCanEquipWeaponType(E_WeaponType.Melee))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Melee].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmo();
                    GamePanelManager.I.SetAmmoActive(true);
                }
                break;
            case 4:
                if (CheckCanEquipWeaponType(E_WeaponType.Grenade))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Grenade].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmo();
                    GamePanelManager.I.SetAmmoActive(true);
                }
                break;
        }
    }

    // 들고자 하는 무기가 들 수 있는 상태인지 확인
    private bool CheckCanEquipWeaponType(E_WeaponType eWeaponType)
    {
        if (m_vCurrentWeapon != null)
        {
            if (m_vCurrentWeapon.a_vWeaponData.a_eWeaponType == eWeaponType)
                return false;  // 지금 들고있는 무기라면 바꿀 필요가 없으니 false
        }

        if (!m_dicWeaponInventory.ContainsKey(eWeaponType))
            return false;   // 들고자 하는 타입의 무기를 갖고 있지 않다면 false

        return true;
    }

    // 플레이어가 땅에 떨어진 무기에 닿으면 해당 무기를 획득한다.
    public void PickUpWeapon(GameObject vWeaponObject)
    {
        WeaponBase vWeaponBase = vWeaponObject.GetComponent<WeaponBase>();
        if(vWeaponBase==null)
        {
            Debug.Log("Debug ERROR : " + vWeaponObject + " weaponBase is NULL");
            return;
        }

        // 만약 닿은 무기의 타입을 이미 가지고 있는 경우, 해당 무기를 획득하지 않는다.
        if(m_dicWeaponInventory.ContainsKey(vWeaponBase.a_vWeaponData.a_eWeaponType))
        {
            Debug.Log("Tried to pick up [" + vWeaponBase.a_vWeaponData.a_strWeaponName + "], But [" + vWeaponBase.a_vWeaponData.a_eWeaponType + "] type is already equiped!");
        }
        else
        {
            m_dicWeaponInventory.Add(vWeaponBase.a_vWeaponData.a_eWeaponType, vWeaponBase);

            int iWeaponViewID = vWeaponObject.GetComponent<PhotonView>().ViewID;  // RPC엔 GameObject를 줄 수 없어서 해당 무기 object의 photon view ID를 대신 준다.
            m_vPhotonView.RPC(nameof(PuckUpWeaponRPC), RpcTarget.AllBuffered, iWeaponViewID);

            // 현재 아무 무기도 들고있지 않은 상태면, 획득한 무기를 즉시 장착한다.
            if (m_vCurrentWeapon == null)
            {
                //pView.RPC(nameof(SetCurrentWeaponRPC), RpcTarget.AllBuffered, weaponViewID);
                a_iCurrentWeaponViewID = iWeaponViewID;
                vWeaponBase.SetAmmo();
                GamePanelManager.I.SetAmmoActive(true);
            }
        }
    }

    // 땅에 떨어진 무기를 플레이어가 가져갔다는 정보를 모든 유저에게 알려준다.
    [PunRPC]
    private void PuckUpWeaponRPC(int iWeaponViewID)
    {
        GameObject vWeaponObject = PhotonView.Find(iWeaponViewID).gameObject;

        vWeaponObject.GetComponent<CapsuleCollider2D>().enabled = false;

        vWeaponObject.transform.parent = gameObject.transform;
        vWeaponObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        vWeaponObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        vWeaponObject.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
