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

            m_vPhotonView.RPC(nameof(base.ShootRPC), RpcTarget.All, m_vMuzzlePosition.position, Quaternion.Euler(0f, 0f, _fAngle), _iShooterActorNumber, m_vWeaponData.a_iWeaponId);

            m_iCurrentAmmo -= 1;
            SetAmmoUI();

            return m_vWeaponData.a_fRecoilIncreaseRate;
        }

        return 0f;
    }

    public override void Reload(WeaponController _vLocalPlayerWeaponController)
    {
        _vLocalPlayerWeaponController.StartCoroutine(_vLocalPlayerWeaponController.ReloadCoroutine(m_vWeaponData.a_fReloadTime));
    }

}
