using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [SerializeField]
    private WeaponData weaponData;
    public WeaponData GetWeaponData { get { return weaponData; } }

    float fireCoolTime;

    int currentAmmo;    // 현재 장전된 총알
    int remainAmmo;     // 남은 총알

    private bool canShooting;

    private void Start()
    {
        InitWeaponData();
    }

    public void InitWeaponData()
    {
        currentAmmo = GetWeaponData.AmmoInMagazine;
        remainAmmo = GetWeaponData.MaxAmmo;
        canShooting = true;
    }

    public void Shoot()
    {
        if (canShooting)
        {
            //PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */
            canShooting = false;
        }
    }

}
