using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public PhotonView pVIew;

    float angle;
    Vector2 target, mouse;

    void Start()
    {
        target = transform.position;
    }

    void Update()
    {
        if (pVIew.IsMine)
        {
            target = transform.position;
            mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            angle = Mathf.Atan2(mouse.y - target.y, mouse.x - target.x) * Mathf.Rad2Deg;
            pVIew.RPC("FlipXRPC", RpcTarget.AllBuffered, angle);   // 재접속시 캐릭터 좌우반전을 동기화하기 위해 AllBuffered
        }
    }

    [PunRPC]
    void FlipXRPC(float angle)
    {
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
