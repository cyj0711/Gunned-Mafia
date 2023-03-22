using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletScript : MonoBehaviourPunCallbacks
{
    public PhotonView PV;
    float bulletSpeed = 4;
    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * bulletSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col)   // col을 RPC의 매개변수로 줄 수 없다.
    {
        if (col.tag == "Blockable") PV.RPC("DestroyRPC", RpcTarget.AllBuffered);

        if (!PV.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine)    // 느린쪽(즉 맞는사람)에 맞춰서 충돌을 판정해 좀더 유저들이 쾌적한 싸움을 경험하게 한다.
        {
            PlayerScript ps = col.GetComponent<PlayerScript>();
            ps.Hit(30);
            PV.RPC("Destroy", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
