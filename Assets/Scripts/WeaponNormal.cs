using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponNormal : WeaponBase
{
    public override float Shoot(float _fAngle, int _iShooterActorNumber)
    {
        if (m_iCurrentAmmo <= 0) return 0f;

        if (DateTime.Now.Subtract(m_vLastShootTime).TotalSeconds >= m_vWeaponData.a_fRateOfFire)
        {
            m_vLastShootTime = DateTime.Now;

            //PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */

            m_vPhotonView.RPC(nameof(ShootRPC), RpcTarget.All, m_vMuzzlePosition.position, Quaternion.Euler(0f, 0f, _fAngle), _iShooterActorNumber, m_vWeaponData.a_iWeaponId);

            m_iCurrentAmmo -= 1;
            SetAmmoUI();

            return m_vWeaponData.a_fRecoilIncreaseRate;
        }

        return 0f;
    }

    [PunRPC]
    public void ShootRPC(Vector3 vPosition, Quaternion vRotation, int iShooterActorNumber, int iWeaponID)
    {
        Instantiate(m_vBulletObject, vPosition, vRotation).GetComponent<BulletController>().SetBulletData(iShooterActorNumber, iWeaponID);
    }

    public override void Reload(WeaponController _vLocalPlayerWeaponController)
    {
        _vLocalPlayerWeaponController.StartCoroutine(_vLocalPlayerWeaponController.ReloadCoroutine(m_vWeaponData.a_fReloadTime));
    }

    //private IEnumerator ReloadCoroutine(WeaponController _vLocalPlayerWeaponController)
    //{
    //    _vLocalPlayerWeaponController.SetBoolIsReloading(true);
    //    _vLocalPlayerWeaponController.a_vReloadUIImage.gameObject.SetActive(true);

    //    float timer = 0f;

    //    while (timer < m_vWeaponData.a_fReloadTime && _vLocalPlayerWeaponController.a_bIsReloading)
    //    {
    //        // 쿨타임 UI 업데이트
    //        float fillAmount = timer / m_vWeaponData.a_fReloadTime;
    //        _vLocalPlayerWeaponController.a_vReloadUIImage.fillAmount = fillAmount;

    //        // 프레임마다 대기
    //        yield return null;

    //        timer += Time.deltaTime;
    //    }

    //    // 장전이 완료된 후에 총알 UI를 최종적으로 업데이트합니다.
    //    if (_vLocalPlayerWeaponController.a_bIsReloading)
    //    {
    //        // if (m_iCurrentAmmo >= m_vWeaponData.a_iAmmoCapacity || m_iRemainAmmo <= 0) return;

    //        int iAmmoNumberToReload = Mathf.Min(m_iRemainAmmo, m_vWeaponData.a_iAmmoCapacity - m_iCurrentAmmo);

    //        m_iRemainAmmo -= iAmmoNumberToReload;
    //        m_iCurrentAmmo += iAmmoNumberToReload;

    //        SetAmmoUI();
    //    }

    //    _vLocalPlayerWeaponController.a_vReloadUIImage.gameObject.SetActive(false);
    //    _vLocalPlayerWeaponController.SetBoolIsReloading(false);
    //}
}
