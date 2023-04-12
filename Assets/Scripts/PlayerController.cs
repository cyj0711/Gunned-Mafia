using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Cinemachine;

public enum E_PlayerRole
{
    None, Civil, Mafia, Detective   // 미참여(관전), 시민, 마피아, 탐정
}

public enum E_PlayerState
{
    Alive, Missing, Dead, Spectator // 생존, 실종, 사망, 관전
}

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable
{
    public Rigidbody2D RB;
    public PhotonView pView;
    public Text NickNameText;
    public Image HealthImage;

    public GameObject weapons;
    public GameObject character;

    CharacterAnimationController characterAnimationController;
    WeaponManager weaponManager;

    public E_PlayerRole PlayerRole { get { return playerRole; } set { playerRole = value; } }
    public E_PlayerState PlayerState { get { return playerState; } set { playerState = value; } }
    private E_PlayerRole playerRole;
    private E_PlayerState playerState;

    int hp = 100;
    int currentHp;

    Vector3 curPos;

    float angle;
    Vector2 target, mouse;

    bool isSeeingRight; // true이면 오른쪽을 보는 상태, false는 왼쪽
    public bool IsSeeingRight { get { return isSeeingRight; } }
    bool lastSeeingRight;

    void Awake()
    {
        NickNameText.text = pView.IsMine ? PhotonNetwork.NickName : pView.Owner.NickName;
        NickNameText.color = pView.IsMine ? Color.green : Color.red;

        characterAnimationController = character.GetComponent<CharacterAnimationController>();
        weaponManager = weapons.GetComponent<WeaponManager>();

        if(pView.IsMine)
        {
            // 2D 카메라
            var CM = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            CM.Follow = transform;
            CM.LookAt = transform;
        }

    }

    private void Start()
    {
        target = transform.position;
        isSeeingRight = true;
        lastSeeingRight = isSeeingRight;

        currentHp = 100;

        PlayerRole = E_PlayerRole.None;
        if (GameManager.I.GameState == E_GAMESTATE.Play || GameManager.I.GameState == E_GAMESTATE.Cooling) PlayerState = E_PlayerState.Spectator;
        else PlayerState = E_PlayerState.Alive; // 게임중이거나 쿨링다운이면 관전으로 입장, 준비중이면 생존상태로 입장
    }

    void Update()
    {
        if(pView.IsMine)
        {
            UpdateWalkingProcess();
            UpdateWeaponAimProcess();
            UpdateWeaponShotProcess();

            //Debug.Log(target.x +" "+ target.y);  // angle의 절대값이 90이하면 오른쪽, 이상이면 왼쪽을 보는것.

            //target = weapons.transform.position;
            //mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //angle = Mathf.Atan2(mouse.y - target.y, mouse.x - target.x) * Mathf.Rad2Deg;
            //pView.RPC("FlipXRPC", RpcTarget.AllBuffered, axisX);   // 재접속시 캐릭터 좌우반전을 동기화하기 위해 AllBuffered
        }

        /* 위치 동기화는 transformView Component를 안 쓰고 OnPhotonSerializeView와 이 코드를 쓰면 빠르고 버그도 없어서 좋다 */
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - curPos).sqrMagnitude >= 100) // 너무 멀리 떨어져있으면 바로 순간이동
            transform.position = curPos;
        else
            transform.position = Vector3.Lerp(transform.position, curPos, Time.deltaTime * 10); // 적당히 떨어져있으면 부드럽게 이동
    }

    //private static int PlayerID = -1;

    //public void SetPlayerID()
    //{
    //    int actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
    //    Player[] sortedPlayers = PhotonNetwork.PlayerList;

    //    for (int i = 0; i < sortedPlayers.Length; i += 1)
    //    {
    //        if (sortedPlayers[i].ActorNumber == actorNumber)
    //        {
    //            PlayerID = i;
    //            break;
    //        }
    //    }

    //    Debug.Log(NickNameText.text + " - PlayerID: " + PlayerID.ToString()+" / ActorNumber: "+actorNumber.ToString());
    //}

    public void Hit(int damage)
    {
        currentHp -= damage;
        HealthImage.fillAmount = (float)(currentHp / (float)hp);

        if(currentHp<=0)
        {
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            pView.RPC("DestroyRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void DestroyRPC() => Destroy(gameObject);

    void WeaponRotation(float angle)
    {
        weapons.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void UpdateWeaponShotProcess()
    {
        if(Input.GetMouseButton(0))
        {
            weaponManager.Shoot(angle);

        }
    }

    void UpdateWalkingProcess()
    {
        // transform을 이동하면 벽에 부딪히면 떨리는 현상이 있으므로 velocity로 움직인다.
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");
        RB.velocity = new Vector2(axisX, axisY);

        if (axisX != 0 || axisY != 0)
        {
            //AN.SetBool("IsWalking", true);
            characterAnimationController.SetWalk(true);
        }
        //else AN.SetBool("IsWalking", false);
        else characterAnimationController.SetWalk(false);
    }

    void UpdateWeaponAimProcess()
    {
        target = transform.position;
        mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        angle = Mathf.Atan2(mouse.y - target.y, mouse.x - target.x) * Mathf.Rad2Deg;

        SetDirection(angle);

        WeaponRotation(angle);
    }

    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    void SetDirection(float angle)
    {
        angle = Mathf.Abs(angle);   // angle의 절대값이 90이하면 오른쪽, 이상이면 왼쪽을 보는것
        if (angle < 90)
            isSeeingRight = true;
        else if (angle > 90)
            isSeeingRight = false;

        if (isSeeingRight != lastSeeingRight)
        {
            pView.RPC("ChangeDirectionRPC", RpcTarget.AllBuffered, isSeeingRight);
        }
        lastSeeingRight = isSeeingRight;
    }

    [PunRPC]
    void ChangeDirectionRPC(bool isSeeingRight)
    {
        Vector3 scale = new Vector3(character.transform.localScale.x, character.transform.localScale.y, character.transform.localScale.z);
        scale.x *= -1;
        character.transform.localScale = scale;
        weapons.GetComponent<WeaponManager>().SetDirection(isSeeingRight);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /* writing일때 구절과 reading일때 구절은 각 줄이 같은 변수를 대하도록 한다.
        이를테면, 첫번쨰 줄의 stream.SendNext(transform.position)와 curPos = (Vector3)stream.ReceiveNext() 은 같은 위치 좌표에 대한 거고
        두번째 줄의 stream.SendNext(HealthImage.fillAmount)와 HealthImage.fillAmount = (float)stream.ReceiveNext();는 같은 체력바에 대한 것이다. */

        if (stream.IsWriting)    // PV가 IsMine 일 때 작동돼서 넘겨줌
        {
            stream.SendNext(transform.position);        // 캐릭터 위치
            stream.SendNext(HealthImage.fillAmount);    // 캐릭터 체력
        }
        else                    // IsMine 아닐 때 작동돼서 받음
        {
            curPos = (Vector3)stream.ReceiveNext();
            HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(pView.IsMine)
        // 땅에 떨어진 무기에 닿으면 해당 무기 획득
        if(collision.tag=="Weapon")
        {
            Debug.Log(collision.gameObject.GetComponent<WeaponBase>().GetWeaponData.WeaponName);

            weaponManager.PickUpWeapon(collision.gameObject);
        }
    }
}
