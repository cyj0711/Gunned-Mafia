using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField]
    private WeaponData m_vWeaponData;
    public WeaponData a_vWeaponData { get { return m_vWeaponData; } }

    [SerializeField]
    private PhotonView m_vPhotonView;
    [SerializeField]
    private Transform m_vMuzzlePosition;
    [SerializeField]
    private GameObject m_vBulletObject;

    int m_iCurrentAmmo;    // 현재 장전된 총알
    int m_iRemainAmmo;     // 남은 총알
    public int a_iCurrentAmmo { get { return m_iCurrentAmmo; } }
    public int a_iRemainAmmo { get { return m_iRemainAmmo; } }

    int m_iOwnerPlayerActorNumber;
    public int a_iOwnerPlayerActorNumber { get { return m_iOwnerPlayerActorNumber; } set { m_iOwnerPlayerActorNumber = value; } }

    DateTime m_vLastShootTime = DateTime.MinValue;

    private void Awake()    // Start로 하면 RPC의 allbuffered로 호출된 함수가 먼저 발동돼서 초기화가 제대로 안되므로 awake를 사용
    {
        InitWeaponData();
    }

    public void InitWeaponData()
    {
        InitCommonData();

        m_iCurrentAmmo = a_vWeaponData.a_iAmmoCapacity;
        m_iRemainAmmo = a_vWeaponData.a_iMaxAmmo;
    }
    public void InitWeaponData(int _iCurrentAmmo, int _iRemainAmmo)
    {
        InitCommonData();

        m_iCurrentAmmo = _iCurrentAmmo;
        m_iRemainAmmo = _iRemainAmmo;
    }

    private void InitCommonData()
    {
        a_iOwnerPlayerActorNumber = -1;
    }

    public void Shoot(float fAngle, int iShooterID)
    {
        if (m_iCurrentAmmo <= 0) return;

        if (DateTime.Now.Subtract(m_vLastShootTime).TotalSeconds >= m_vWeaponData.a_fRateOfFire)
        {
            m_vLastShootTime = DateTime.Now;

            //PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */

            m_vPhotonView.RPC(nameof(ShootRPC), RpcTarget.All, m_vMuzzlePosition.position, Quaternion.Euler(0f, 0f, fAngle), iShooterID, m_vWeaponData.a_iWeaponId);

            m_iCurrentAmmo -= 1;
            SetAmmoUI();
        }
    }

    [PunRPC]
    public void ShootRPC(Vector3 vPosition, Quaternion vRotation, int iShooterID, int iWeaponID)
    {
        Instantiate(m_vBulletObject, vPosition, vRotation).GetComponent<BulletController>().SetBulletData(iShooterID, iWeaponID);
    }

    public void Reload()
    {
        if (m_iCurrentAmmo >= m_vWeaponData.a_iAmmoCapacity || m_iRemainAmmo <= 0) return;

        int iAmmoNumberToReload = Mathf.Min(m_iRemainAmmo, m_vWeaponData.a_iAmmoCapacity - m_iCurrentAmmo);

        m_iRemainAmmo -= iAmmoNumberToReload;
        m_iCurrentAmmo += iAmmoNumberToReload;

        SetAmmoUI();
    }

    public void SetAmmoUI()
    {
        GamePanelManager.I.SetAmmo(m_vWeaponData.a_iAmmoCapacity, m_iCurrentAmmo, m_iRemainAmmo);
    }

    public void ThrowOutWeapon()
    {
        StartCoroutine(ColliderOnCoroutine());
    }

    private IEnumerator ColliderOnCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        gameObject.GetComponent<CapsuleCollider2D>().enabled = true;
    }
}
