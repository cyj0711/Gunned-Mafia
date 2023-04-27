using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class BulletController : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView m_vPhotonView;
    float m_fBulletSpeed = 4;

    int m_iShooterID = -1;
    int m_iWeaponID = -1;
    WeaponData m_vWeaponData;

    void Start()
    {
        Destroy(gameObject, 3.5f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * m_fBulletSpeed * Time.deltaTime);
    }

    public void SetBulletData(int _iShooterID, int _iWeaponID)
    {
        m_iShooterID = _iShooterID;
        m_iWeaponID = _iWeaponID;
    }

    private void OnTriggerEnter2D(Collider2D col)   // col을 RPC의 매개변수로 줄 수 없다.
    {
        //if (col.tag == "Blockable")
        //{
        //     pView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        //}

        //if (!pView.IsMine && col.tag == "Player" && col.GetComponent<PhotonView>().IsMine)    // 느린쪽(즉 맞는사람)에 맞춰서 충돌을 판정해 좀더 유저들이 쾌적한 싸움을 경험하게 한다.
        //{
        //    PlayerController ps = col.GetComponentInParent<PlayerController>();
        //    ps.Hit(30);
        //    pView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        //}

        if (col.tag == "Blockable")
        {
            Destroy(gameObject);
        }

        if (col.tag == "Player")    // 느린쪽(즉 맞는사람)에 맞춰서 충돌을 판정해 좀더 유저들이 쾌적한 싸움을 경험하게 한다.
        {
            if(col.GetComponent<PhotonView>().IsMine)
            {
                PlayerController player = col.GetComponentInParent<PlayerController>();
                player.Hit(m_vWeaponData.a_iDamage);
            }
            Destroy(gameObject);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);
}
