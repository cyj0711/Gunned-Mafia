using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public Animator AN;
    public SpriteRenderer SR;
    public PhotonView PV;
    public Text NickNameText;
    public Image HealthImage;

    Vector3 curPos;

    float angle;
    public GameObject weapons;
    Vector2 target, mouse;

    void Awake()
    {
        NickNameText.text = PV.IsMine ? PhotonNetwork.NickName : PV.Owner.NickName;
        NickNameText.color = PV.IsMine ? Color.green : Color.red;
    }

    private void Start()
    {
        target = weapons.transform.position;
    }

    void Update()
    {
        if(PV.IsMine)
        {
            // transform을 이동하면 벽에 부딪히면 떨리는 현상이 있으므로 velocity로 움직인다.
            float axisX = Input.GetAxisRaw("Horizontal");
            float axisY = Input.GetAxisRaw("Vertical");
            RB.velocity = new Vector2(axisX, axisY);

            if (axisX != 0 || axisY != 0)
            {
                AN.SetBool("IsWalking", true);
            }
            else AN.SetBool("IsWalking", false);

            //Debug.Log(target.x +" "+ target.y);  // angle의 절대값이 90이하면 오른쪽, 이상이면 왼쪽을 보는것.

            //target = weapons.transform.position;
            //mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //angle = Mathf.Atan2(mouse.y - target.y, mouse.x - target.x) * Mathf.Rad2Deg;
            //PV.RPC("FlipXRPC", RpcTarget.AllBuffered, axisX);   // 재접속시 캐릭터 좌우반전을 동기화하기 위해 AllBuffered
        }
    }

    [PunRPC]
    void FlipXRPC(float axisX)
    {
        weapons.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }
}
