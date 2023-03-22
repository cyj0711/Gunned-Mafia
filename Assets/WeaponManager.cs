using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView pView;

    public List<GameObject> weapons = new List<GameObject>();

    public Transform muzzlePosition;

    private float rateOfFire = 0.1f;
    float fireCoolTime;

    private bool canShooting = true;


    void Start()
    {
        fireCoolTime = rateOfFire;
    }

    void Update()
    {
        CheckCanShooting();

    }

    // 연사속도를 통해 연사 조절
    private void CheckCanShooting()
    {
        if (canShooting == false)
        {
            if (fireCoolTime <= 0f)
            {
                canShooting = true;
                fireCoolTime = rateOfFire;
            }
            else
            {
                fireCoolTime -= Time.deltaTime;
            }
        }
    }

    // 플레이어가 마우스를 누르면 이 함수가 호출되어 총알 발사
    public void Shoot(float angle)
    {
        if (canShooting)
        {
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */
            canShooting = false;
        }
    }


    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    public void SetDirection(bool isSeeingRight)
    {
        for(int i=0;i<weapons.Count;i++)
        {
            weapons[i].GetComponent<SpriteRenderer>().flipY = !isSeeingRight;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
