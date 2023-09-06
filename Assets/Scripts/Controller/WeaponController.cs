using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

public class WeaponController : MonoBehaviourPunCallbacks , IPunObservable
{
    [SerializeField]
    private PhotonView m_vPhotonView;

    private Dictionary<E_WeaponType, WeaponBase> m_dicWeaponInventory = new Dictionary<E_WeaponType, WeaponBase>();

    private WeaponBase m_vCurrentWeapon;

    private bool m_bSeeingRight = true;
    private bool m_bIsShooting = false;
    private bool m_bIsAiming = false;

    private Coroutine m_coToggleAim;
    //private Vector3 m_vLastMousePosition;

    //*************** Synchronization Properties *******************
    [SerializeField] int m_iCurrentWeaponViewID;   // 아무것도 안들었을땐 -1 로 설정해주자.
    public int a_iCurrentWeaponViewID { get => m_iCurrentWeaponViewID; set => SetPropertyRPC(nameof(CurrentWeaponIDRPC), value); }
    [PunRPC] void CurrentWeaponIDRPC(int value)
    { 
        m_iCurrentWeaponViewID = value;

        if (a_iCurrentWeaponViewID == -1) 
        { 
            m_vCurrentWeapon = null; 
            return; 
        }

        GameObject vWeaponObject = PhotonView.Find(m_iCurrentWeaponViewID).gameObject;
        if (m_vCurrentWeapon != null)
        {
            m_vCurrentWeapon.gameObject.SetActive(false);   // 바꾸기 전 무기 오브젝트를 끈다.
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
        a_iCurrentWeaponViewID = m_iCurrentWeaponViewID;
    }

    void Start()
    {
        //InitWeaponController();
    }

    public void InitWeaponController()
    {
        m_vPhotonView.RPC(nameof(InitWeaponControllerRPC),RpcTarget.AllBuffered);
        a_iCurrentWeaponViewID = -1;
        m_bIsShooting = false;
        m_bIsAiming = false;
    }

    [PunRPC]
    private void InitWeaponControllerRPC()
    {
        m_dicWeaponInventory.Clear();
    }

    // 플레이어가 마우스를 누르면 이 함수가 호출되어 총알 발사
    public void Shoot()
    {
        if(m_vCurrentWeapon!=null)
        {
            if (m_vCurrentWeapon.a_vWeaponData.a_bAutoFire || !m_bIsShooting)
            {
                m_vCurrentWeapon.Shoot(transform.rotation.eulerAngles.z, PhotonNetwork.LocalPlayer.ActorNumber);
                m_bIsShooting = true;
            }
        }
    }
    public void StopShooting()
    {
        m_bIsShooting = false;
    }

    public void ToggleAim()
    {
        ToggleAim(!m_bIsAiming);
    }

    // 무기의 조준을 on off한다.
    public void ToggleAim(bool _bIsAiming)
    {
        if (m_vCurrentWeapon == null) return;

        m_bIsAiming = _bIsAiming;

        // 조준
        if (m_bIsAiming)
        {
            if (m_coToggleAim == null)
            {
                m_coToggleAim = StartCoroutine(AimCoroutine());
            }
        }
        // 조준 해제
        else
        {
            if (m_coToggleAim != null)
            {
                StopCoroutine(m_coToggleAim);
                m_coToggleAim = null;
            }

            CameraManager.I.SetCameraFollowerPosition(Vector3.zero);
        }
    }

    private IEnumerator AimCoroutine()
    {
        Vector3 m_vLastMousePosition = Input.mousePosition;
        bool bIsClickedFrame = true;    // AimCoroutine 이 호출된 바로 그 프레임인지 확인(우클릭 누르자마자 바로 조준하는 용도)

        while (m_bIsAiming)
        {
            Vector3 vCurrentMousePosition = Input.mousePosition;

            // if (Vector3.Distance(m_vLastMousePosition, vCurrentMousePosition) > 0.2f 의  0.2f 는 보정값임.
            if (Vector3.Distance(m_vLastMousePosition, vCurrentMousePosition) > 0.2f || bIsClickedFrame)
            {
                Vector3 vAimPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
            Input.mousePosition.y, -Camera.main.transform.position.z)) - transform.parent.transform.position;

                // 조준하고자 하는 마우스 위치와 플레이어의 거리는 무기의 조준 임계치를 넘을 수 없다.
                if (vAimPosition.magnitude > m_vCurrentWeapon.a_vWeaponData.a_fZoomFactor)
                {
                    vAimPosition = vAimPosition.normalized * m_vCurrentWeapon.a_vWeaponData.a_fZoomFactor;
                }

                CameraManager.I.SetCameraFollowerPosition(vAimPosition);

            }
            m_vLastMousePosition = vCurrentMousePosition;
            bIsClickedFrame = false;

            yield return null;
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

    // 플레이어가 사망하면 모든 무기를 떨어트린다.
    public void DropAllWeapons()
    {
        int i = 0;
        float fDropRotation = 360f / m_dicWeaponInventory.Count;

        // foreach 문에서 Dictionary.remove를 진행(RPC 함수에서 진행함)하기 때문에(DropWeaponRPC에서) in m_dicWeaponInventory가 아닌 in m_dicWeaponInventory.Keys.ToList()로 진행
        foreach (var _dicWeaponInventory in m_dicWeaponInventory.Keys.ToList())
        {
            if (m_dicWeaponInventory.TryGetValue(_dicWeaponInventory, out WeaponBase _vCurrentWeapon))
            {
                m_vPhotonView.RPC(nameof(DropWeaponRPC), RpcTarget.All,
                    _vCurrentWeapon.GetPhotonViewID(), _vCurrentWeapon.a_iCurrentAmmo, _vCurrentWeapon.a_iRemainAmmo, _vCurrentWeapon.transform.position, Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, i * fDropRotation)));
                i++;
            }
        }
        InitWeaponController();
        UIGameManager.I.SetAmmoActive(false);
    }

    // 플레이어가 게임을 종료할 경우 모든 무기를 버린다 (게임을 종료하면 photon view 가 없어져 RPC를 못쓰기에 DropAllWeapons() 대신 해당 메소드 사용)
    public void DropAllWeaponsOnLeft()
    {
        int i = 0;
        float fDropRotation = 360f / m_dicWeaponInventory.Count;

        foreach (KeyValuePair<E_WeaponType, WeaponBase> _dicWeaponInventory in m_dicWeaponInventory)
        {
            WeaponBase vCurrentWeapon = _dicWeaponInventory.Value;

            vCurrentWeapon.InitWeaponData(vCurrentWeapon.a_iCurrentAmmo, vCurrentWeapon.a_iRemainAmmo);

            vCurrentWeapon.transform.parent = MapManager.I.a_vDroppedItem;
            vCurrentWeapon.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            if (vCurrentWeapon.gameObject.transform.localScale.y < 0)
                vCurrentWeapon.gameObject.transform.localScale = new Vector3
                    (vCurrentWeapon.gameObject.transform.localScale.x, vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);

            vCurrentWeapon.gameObject.SetActive(true);

            vCurrentWeapon.DropWeapon(Quaternion.Euler(new Vector3(0f, 0f, i * fDropRotation))); 
            
            i++;
        }

        InitWeaponController();
    }


    // 키보드 g를 누르면 현재 들고있는 무기를 땅에 버린다.
    public void DropWeapon()
    {
        if (m_vCurrentWeapon == null)
            return;

        // m_dicWeaponInventory.Remove(m_vCurrentWeapon.a_vWeaponData.a_eWeaponType);
        ToggleAim(false);

        m_vPhotonView.RPC(nameof(DropWeaponRPC), RpcTarget.AllBuffered, 
            m_iCurrentWeaponViewID, m_vCurrentWeapon.a_iCurrentAmmo, m_vCurrentWeapon.a_iRemainAmmo, m_vCurrentWeapon.transform.position, transform.rotation);

        a_iCurrentWeaponViewID = -1;
        UIGameManager.I.SetAmmoActive(false);
    }

    [PunRPC]
    private void DropWeaponRPC(int _iCurrentWeaponViewID, int _iCurrentAmmo, int _iRemainAmmo, Vector3 _vWeaponPosition, Quaternion _qPlayerAimRotation)
    {
        PhotonView vWeaponPhotonView = PhotonView.Find(_iCurrentWeaponViewID);
        if (vWeaponPhotonView == null || vWeaponPhotonView.gameObject == null) { return; }

        WeaponBase vCurrentWeapon = vWeaponPhotonView.GetComponent<WeaponBase>();

        m_dicWeaponInventory.Remove(vCurrentWeapon.a_vWeaponData.a_eWeaponType);

        vCurrentWeapon.InitWeaponData(_iCurrentAmmo, _iRemainAmmo);

        vCurrentWeapon.transform.parent = MapManager.I.a_vDroppedItem;
        vCurrentWeapon.transform.position = _vWeaponPosition;
        vCurrentWeapon.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        if (vCurrentWeapon.gameObject.transform.localScale.y < 0)
            vCurrentWeapon.gameObject.transform.localScale = new Vector3
                (vCurrentWeapon.gameObject.transform.localScale.x, vCurrentWeapon.gameObject.transform.localScale.y * -1, 1);

        vCurrentWeapon.gameObject.SetActive(true);

        vCurrentWeapon.DropWeapon(_qPlayerAimRotation);
    }

    // 숫자키를 입력받으면 그에 해당하는 무기로 변경
    public void ChangeCurrentWeapon(int _iInputKeyNumber)
    {
        // 교체하려는 무기가 현재 들고있으면 무시함
        if (m_vCurrentWeapon != null && _iInputKeyNumber == ((int)m_vCurrentWeapon.a_vWeaponData.a_eWeaponType + 1))
            return;

        StopShooting();
        ToggleAim(false);

        switch(_iInputKeyNumber)
        {
            case 1:
                if(CheckCanEquipWeaponType(E_WeaponType.Primary))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Primary].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmoUI();
                    UIGameManager.I.SetAmmoActive(true);
                }
                break;
            case 2:
                if (CheckCanEquipWeaponType(E_WeaponType.Secondary))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Secondary].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmoUI();
                    UIGameManager.I.SetAmmoActive(true);
                }
                break;
            case 3:
                if (CheckCanEquipWeaponType(E_WeaponType.Melee))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Melee].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmoUI();
                    UIGameManager.I.SetAmmoActive(true);
                }
                break;
            case 4:
                if (CheckCanEquipWeaponType(E_WeaponType.Grenade))
                {
                    a_iCurrentWeaponViewID = m_dicWeaponInventory[E_WeaponType.Grenade].gameObject.GetComponent<PhotonView>().ViewID;
                    m_vCurrentWeapon.SetAmmoUI();
                    UIGameManager.I.SetAmmoActive(true);
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
    public void CheckPickUpWeapon(GameObject vWeaponObject)
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
            int iWeaponViewID = vWeaponObject.GetComponent<PhotonView>().ViewID;  // RPC엔 GameObject를 줄 수 없어서 해당 무기 object의 photon view ID를 대신 준다.

            GameManager.I.CheckCanPlayerPickUpWeapon(iWeaponViewID, m_vPhotonView.Owner.ActorNumber); // 서버의 GameManager가 무기 획득 여부를 확인한뒤 클라이언트의 PickUpWeapon을 호출

        }
    }

    public void PickUpWeapon(int _iWeaponViewID)
    {
        WeaponBase vWeaponBase = PhotonView.Find(_iWeaponViewID).gameObject.GetComponent<WeaponBase>();

        //m_dicWeaponInventory.Add(vWeaponBase.a_vWeaponData.a_eWeaponType, vWeaponBase);

        m_vPhotonView.RPC(nameof(PuckUpWeaponRPC), RpcTarget.AllBuffered, _iWeaponViewID);

        // 현재 아무 무기도 들고있지 않은 상태면, 획득한 무기를 즉시 장착한다.
        if (m_vCurrentWeapon == null)
        {
            //pView.RPC(nameof(SetCurrentWeaponRPC), RpcTarget.AllBuffered, weaponViewID);
            a_iCurrentWeaponViewID = _iWeaponViewID;
            vWeaponBase.SetAmmoUI();
            UIGameManager.I.SetAmmoActive(true);
        }

        UIGameManager.I.SendNotification("You picked up " + vWeaponBase.a_vWeaponData.name);
    }

    // 땅에 떨어진 무기를 플레이어가 가져갔다는 정보를 모든 유저에게 알려준다.
    [PunRPC]
    private void PuckUpWeaponRPC(int iWeaponViewID)
    {
        // GameObject vWeaponObject = PhotonView.Find(iWeaponViewID).gameObject;
        PhotonView vWeaponPhotonView = PhotonView.Find(iWeaponViewID);

        if (vWeaponPhotonView == null || vWeaponPhotonView.gameObject == null) { return; }

        WeaponBase vWeaponBase = vWeaponPhotonView.GetComponent<WeaponBase>();

        vWeaponBase.a_iOwnerPlayerActorNumber = m_vPhotonView.OwnerActorNr;
        vWeaponBase.SetWeaponCollider(false);

        vWeaponBase.transform.parent = gameObject.transform;
        vWeaponBase.transform.localPosition = new Vector3(0f, 0f, 0f);
        vWeaponBase.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        vWeaponBase.gameObject.SetActive(false);

        m_dicWeaponInventory.Add(vWeaponBase.a_vWeaponData.a_eWeaponType, vWeaponBase);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
