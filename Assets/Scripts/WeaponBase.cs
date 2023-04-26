using Photon.Pun;
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

    float m_fFireTimer;

    int m_iCurrentAmmo;    // 현재 장전된 총알
    int m_iRemainAmmo;     // 남은 총알

    bool m_bCanShooting;

    private void Start()
    {
        InitWeaponData();
    }

    public void InitWeaponData()
    {
        m_iCurrentAmmo = a_vWeaponData.a_iAmmoCapacity;
        m_iRemainAmmo = a_vWeaponData.a_iMaxAmmo;
        m_bCanShooting = true;
    }

    void Update()
    {
        CheckCanShooting();

        //Debug.Log(gameObject.transform.eulerAngles.z);
        //if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
        //    SetDirection(false);
        //else if (transform.eulerAngles.z < 90 || transform.eulerAngles.z > 270)
        //    SetDirection(true);
    }

    // 연사속도를 통해 연사 조절
    private void CheckCanShooting()
    {
        if (m_bCanShooting == false)
        {
            if (m_fFireTimer <= 0f)
            {
                m_bCanShooting = true;
                m_fFireTimer = m_vWeaponData.a_fRateOfFire;
            }
            else
            {
                m_fFireTimer -= Time.deltaTime;
            }
        }
    }


    public void Shoot(float fAngle)
    {
        if (m_iCurrentAmmo <= 0) return;

        if (m_bCanShooting)
        {
            m_bCanShooting = false;
            //PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */

            m_vPhotonView.RPC(nameof(ShootRPC), RpcTarget.All, m_vMuzzlePosition.position, Quaternion.Euler(0f, 0f, fAngle));
            //Instantiate(bullet, muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            m_iCurrentAmmo -= 1;
            SetAmmo();

        }
    }

    public void Reload()
    {
        if (m_iCurrentAmmo >= m_vWeaponData.a_iAmmoCapacity || m_iRemainAmmo <= 0) return;

        int iAmmoNumberToReload = Mathf.Min(m_iRemainAmmo, m_vWeaponData.a_iAmmoCapacity - m_iCurrentAmmo);

        m_iRemainAmmo -= iAmmoNumberToReload;
        m_iCurrentAmmo += iAmmoNumberToReload;

        SetAmmo();
    }

    public void SetAmmo()
    {
        GamePanelManager.I.SetAmmo(m_vWeaponData.a_iAmmoCapacity, m_iCurrentAmmo, m_iRemainAmmo);
    }

    [PunRPC]
    public void ShootRPC(Vector3 vPosition, Quaternion vRotation)
    {
        Instantiate(m_vBulletObject, vPosition, vRotation);
    }
}
