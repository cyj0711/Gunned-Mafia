using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField]
    protected WeaponData m_vWeaponData;
    public WeaponData a_vWeaponData { get { return m_vWeaponData; } }

    [SerializeField]
    protected PhotonView m_vPhotonView;
    [SerializeField]
    protected Transform m_vMuzzlePosition;
    [SerializeField]
    protected GameObject m_vBulletObject;
    [SerializeField]
    private GameObject m_vWeaponSkinObject;
    [SerializeField]
    private Rigidbody2D m_vRigidbody;

    private Collider2D m_vWeaponSkinCollider;

    protected int m_iCurrentAmmo;    // 현재 장전된 총알
    protected int m_iRemainAmmo;     // 남은 총알
    public int a_iCurrentAmmo { get { return m_iCurrentAmmo; } }
    public int a_iRemainAmmo { get { return m_iRemainAmmo; } }

    int m_iOwnerPlayerActorNumber;  // 이 무기를 들고있는 플레이어
    public int a_iOwnerPlayerActorNumber { get { return m_iOwnerPlayerActorNumber; } set { m_iOwnerPlayerActorNumber = value; } }

    int m_iWeaponID;
    public int a_iWeaponID { get { return m_iWeaponID; } set { m_iWeaponID = value; } }

    protected DateTime m_vLastShootTime = DateTime.MinValue;

    private void Awake()    // Start로 하면 RPC의 allbuffered로 호출된 함수가 먼저 발동돼서 초기화가 제대로 안되므로 awake를 사용
    {
        //InitWeaponData();
    }

    public void InitWeaponData()
    {
        InitCommonData();

        m_iCurrentAmmo = m_vWeaponData.a_iAmmoCapacity;
        m_iRemainAmmo = m_vWeaponData.a_iMaxAmmo;
    }

    // 남이 사용하고 버린 무기를 주울 경우 탄창을 동기화한다.
    public void InitWeaponData(int _iCurrentAmmo, int _iRemainAmmo)
    {
        InitCommonData();

        m_iCurrentAmmo = _iCurrentAmmo;
        m_iRemainAmmo = _iRemainAmmo;
    }

    private void InitCommonData()
    {
        SetWeaponSkin();
        m_iOwnerPlayerActorNumber = -1;
    }

    private void SetWeaponSkin()
    {
        if (m_vWeaponSkinObject == null)
        {
            m_vWeaponSkinObject = Instantiate(m_vWeaponData.a_vWeaponPrefab);
            m_vWeaponSkinObject.transform.parent = transform;
            m_vWeaponSkinObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            m_vWeaponSkinObject.transform.rotation = Quaternion.identity;

            m_vMuzzlePosition = m_vWeaponSkinObject.transform.Find("ShootPosition");

            m_vWeaponSkinCollider = m_vWeaponSkinObject.GetComponent<Collider2D>();
        }
    }

    // return weapon's recoil (0f if don't shoot)
    public abstract float Shoot(float _fAngle, int _iShooterActorNumber);

    [PunRPC]
    public void ShootRPC(Vector3 vPosition, Quaternion vRotation, int iShooterActorNumber, int iWeaponID)
    {
        GameObject vPooledBullet = ObjectPoolingManager.I.a_vBulletObjectPool.ActivatePoolItem();
        if (vPooledBullet != null)
            vPooledBullet.GetComponent<BulletController>().SetBulletData(iShooterActorNumber, iWeaponID, vPosition, vRotation, ObjectPoolingManager.I.a_vBulletObjectPool);
        // Instantiate(m_vBulletObject, vPosition, vRotation).GetComponent<BulletController>().SetBulletData(iShooterActorNumber, iWeaponID);
    }

    //[PunRPC]
    //public void ShootRPC(Vector3 vPosition, Quaternion vRotation, int iShooterActorNumber, int iWeaponID)
    //{
    //    Instantiate(m_vBulletObject, vPosition, vRotation).GetComponent<BulletController>().SetBulletData(iShooterActorNumber, iWeaponID);
    //}

    public abstract void Reload(WeaponController _vLocalPlayerWeaponController);

    public virtual void StopReload() { }

    public virtual void SetReloadAmmo()
    {
        if (m_iCurrentAmmo >= m_vWeaponData.a_iAmmoCapacity || m_iRemainAmmo <= 0) return;

        int iAmmoNumberToReload = Mathf.Min(m_iRemainAmmo, m_vWeaponData.a_iAmmoCapacity - m_iCurrentAmmo);

        m_iRemainAmmo -= iAmmoNumberToReload;
        m_iCurrentAmmo += iAmmoNumberToReload;

        SetAmmoUI();
    }

    public void SetAmmoUI()
    {
        UIGameManager.I.SetAmmo(m_vWeaponData.a_iAmmoCapacity, m_iCurrentAmmo, m_iRemainAmmo);
    }

    public void DropWeapon(Quaternion _WeaponRoration)
    {
        StartCoroutine(SmoothLerp(0.2f, _WeaponRoration));
    }

    public void SetWeaponCollider(bool _bIsEnable)
    {
        m_vWeaponSkinCollider.enabled = _bIsEnable;
    }

    private IEnumerator SmoothLerp(float time, Quaternion _WeaponRoration)
    {
        Vector3 startingPos = transform.position;
        Vector3 finalPos = transform.position + (_WeaponRoration.normalized * Vector3.right * 0.4f);
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(time);
        SetWeaponCollider(true);
    }

    // 게임 도중 관전자 플레이어가 들어왔을 때 게임 내 무기들의 위치를 바로잡는다.
    private void SetPosition()
    {
        m_vPhotonView.RPC(nameof(SetPositionRPC), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);
    }

    [PunRPC]
    public void SetPositionRPC(int _iLocalPlayerActorNumber)
    {
        m_vPhotonView.RPC(nameof(ReturnPositionRPC), PhotonNetwork.CurrentRoom.GetPlayer(_iLocalPlayerActorNumber), m_iOwnerPlayerActorNumber, transform.position);
    }

    [PunRPC]
    public void ReturnPositionRPC(int _iOwnerPlayerActorNumber, Vector3 _vPosition)
    {
        if (_iOwnerPlayerActorNumber == -1)
        {
            transform.parent = MapManager.I.a_vDroppedItem;
            transform.position = _vPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            gameObject.SetActive(true);
        }
        else
        {

        }
    }


    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] vInstantiationData = info.photonView.InstantiationData;

        if (vInstantiationData != null && vInstantiationData.Length == 1)   // vInstantiationData 에는 weaponID 밖에 들어있지 않으므로 Length==1이다.
        {
            m_vWeaponData = DataManager.I.GetWeaponDataWithID((int)vInstantiationData[0]);
            InitWeaponData();

            SetPosition();

            MapManager.I.AddListWeapon(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public int GetPhotonViewID()
    {
        return m_vPhotonView.ViewID;
    }

}
