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

public class PlayerController : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] Rigidbody2D m_vRigidBody;
    [SerializeField] PhotonView m_vPhotonView;
    [SerializeField] Text m_vNickNameText;
    [SerializeField] Image m_vHealthImage;

    //public GameObject weapons;
    [SerializeField] GameObject m_vCharacterObject;

    CharacterAnimationController m_vCharacterAnimationController;
    [SerializeField] WeaponManager m_vWeaponManager;

    private E_PlayerRole m_ePlayerRole;
    private E_PlayerState m_ePlayerState;
    public E_PlayerRole a_ePlayerRole { get { return m_ePlayerRole; } set { m_ePlayerRole = value; } }
    public E_PlayerState a_ePlayerState { get { return m_ePlayerState; } set { m_ePlayerState = value; } }

    private int m_iLastAttackerID = -1;         // 최근에 플레이어를 공격한 플레이어 id
    private int m_iLastDamagedWeaponID = -1;    // 최근에 플레이어를 공격한 무기 id
    public int a_iLastAttackerID { get { return m_iLastAttackerID; } set { m_iLastAttackerID = value; } }
    public int a_iLastDamagedWeaponID { get { return m_iLastDamagedWeaponID; } set { m_iLastDamagedWeaponID = value; } }

    //*************** Synchronization Properties *******************
    [SerializeField] int m_iCurrentHealth;
    public int a_iCurrentHealth { get => m_iCurrentHealth; set => SetPropertyRPC(nameof(SetCurrentHealthRPC), value); }
    [PunRPC] void SetCurrentHealthRPC(int value) { m_iCurrentHealth = value; m_vHealthImage.fillAmount = (float)(a_iCurrentHealth / 100f);}
    //**************************************************************

    void SetPropertyRPC(string functionName, object value)
    {
        m_vPhotonView.RPC(functionName, RpcTarget.All, value);
    }

    public void InvokeProperties()  // Synchronization properites에 새 속성을 넣을경우 여기에 반드시 추가한다.(변수명이 아닌 get set 명임!!)
    {
        a_iCurrentHealth = a_iCurrentHealth;
    }

    Vector3 m_vCurrentPosition;

    float m_fAngle;
    Vector2 m_vTargetPosition, m_vMousePosition;

    bool m_bSeeingRight; // true이면 오른쪽을 보는 상태, false는 왼쪽
    public bool a_bSeeingRight { get { return m_bSeeingRight; } }
    bool m_bLastSeeingRight;

    void Awake()
    {
        m_vNickNameText.text = m_vPhotonView.IsMine ? PhotonNetwork.NickName : m_vPhotonView.Owner.NickName;
        m_vNickNameText.color = m_vPhotonView.IsMine ? Color.green : Color.red;

        m_vCharacterAnimationController = m_vCharacterObject.GetComponent<CharacterAnimationController>();

        if(m_vPhotonView.IsMine)
        {
            // 2D 카메라
            var vCinemachineCamera = GameObject.Find("CMCamera").GetComponent<CinemachineVirtualCamera>();
            vCinemachineCamera.Follow = transform;
            vCinemachineCamera.LookAt = transform;
        }

    }

    private void Start()
    {
        m_vTargetPosition = transform.position;
        m_bSeeingRight = true;
        m_bLastSeeingRight = m_bSeeingRight;

        a_ePlayerRole = E_PlayerRole.None;
        if (GameManager.I.a_eGameState == E_GAMESTATE.Play || GameManager.I.a_eGameState == E_GAMESTATE.Cooling) a_ePlayerState = E_PlayerState.Spectator;
        else a_ePlayerState = E_PlayerState.Alive; // 게임중이거나 쿨링다운이면 관전으로 입장, 준비중이면 생존상태로 입장
    }

    private void InitialSetting()   // 처음에는 Serializefield를 통해 에디터에서 값을 줬으므로 Start에선 사용하지않는다.
    {
        a_iCurrentHealth = 100;
    }

    void Update()
    {
        if(m_vPhotonView.IsMine)
        {
            UpdateWalkingProcess();
            UpdateWeaponAimProcess();
            UpdateWeaponShotProcess();
            UpdateKeyboardInputProcess();
        }

        /* 위치 동기화는 transformView Component를 안 쓰고 OnPhotonSerializeView와 이 코드를 쓰면 빠르고 버그도 없어서 좋다 */
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - m_vCurrentPosition).sqrMagnitude >= 100) // 너무 멀리 떨어져있으면 바로 순간이동
            transform.position = m_vCurrentPosition;
        else
            transform.position = Vector3.Lerp(transform.position, m_vCurrentPosition, Time.deltaTime * 10); // 적당히 떨어져있으면 부드럽게 이동
    }

    public void Hit(int damage, int _iShooterID, int _iWeaponID)
    {
        a_iCurrentHealth = a_iCurrentHealth - damage;
        a_iLastAttackerID = _iShooterID;
        a_iLastDamagedWeaponID = _iWeaponID;

        if (a_iCurrentHealth <= 0)
        {
            //Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is killed by " + PhotonNetwork.CurrentRoom.GetPlayer(_iShooterID).NickName + " with " + DataManager.I.GetWeaponDataWithID(_iWeaponID).a_strWeaponName);
            GameObject.Find("Canvas").transform.Find("RespawnPanel").gameObject.SetActive(true);
            m_vPhotonView.RPC(nameof(DestroyRPC), RpcTarget.AllBuffered, _iShooterID, _iWeaponID);
        }
    }

    [PunRPC]
    //void DestroyRPC() => Destroy(gameObject);
    void DestroyRPC(int _iShooterID, int _iWeaponID)
    {
        Debug.Log(m_vPhotonView.Owner.NickName + " is killed by " + PhotonNetwork.CurrentRoom.GetPlayer(_iShooterID).NickName + " with " + DataManager.I.GetWeaponDataWithID(_iWeaponID).a_strWeaponName);
        Destroy(gameObject);
    }

    void WeaponRotation(float fAngle)
    {
        m_vWeaponManager.gameObject.transform.rotation = Quaternion.AngleAxis(fAngle, Vector3.forward);
    }

    void UpdateWeaponShotProcess()
    {
        if(Input.GetMouseButton(0))
        {
            m_vWeaponManager.Shoot();

        }
    }

    void UpdateKeyboardInputProcess()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            m_vWeaponManager.Reload();
        }

        else if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            m_vWeaponManager.ChangeCurrentWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            m_vWeaponManager.ChangeCurrentWeapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            m_vWeaponManager.ChangeCurrentWeapon(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            m_vWeaponManager.ChangeCurrentWeapon(4);
        }

        else if(Input.GetKeyDown(KeyCode.G))
        {
            m_vWeaponManager.ThrowOutWeapon();
        }
    }

    //public int GetPressedNumber()
    //{
    //    for (int number = 0; number <= 9; number++)
    //    {
    //        if (Input.GetKeyDown(number.ToString()))
    //            return number;
    //    }

    //    return -1;
    //}

    void UpdateWalkingProcess()
    {
        // transform을 이동하면 벽에 부딪히면 떨리는 현상이 있으므로 velocity로 움직인다.
        float axisX = Input.GetAxisRaw("Horizontal");
        float axisY = Input.GetAxisRaw("Vertical");
        m_vRigidBody.velocity = new Vector2(axisX, axisY);

        if (axisX != 0 || axisY != 0)
        {
            m_vCharacterAnimationController.SetWalk(true);
        }
        else m_vCharacterAnimationController.SetWalk(false);
    }

    void UpdateWeaponAimProcess()
    {
        m_vTargetPosition = transform.position;
        m_vMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        m_fAngle = Mathf.Atan2(m_vMousePosition.y - m_vTargetPosition.y, m_vMousePosition.x - m_vTargetPosition.x) * Mathf.Rad2Deg;

        SetDirection(m_fAngle);

        WeaponRotation(m_fAngle);
    }

    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    void SetDirection(float angle)
    {
        angle = Mathf.Abs(angle);   // angle의 절대값이 90이하면 오른쪽, 이상이면 왼쪽을 보는것
        if (angle < 90)
            m_bSeeingRight = true;
        else if (angle > 90)
            m_bSeeingRight = false;

        if (m_bSeeingRight != m_bLastSeeingRight)
        {
            m_vPhotonView.RPC(nameof(ChangeDirectionRPC), RpcTarget.AllBuffered, m_bSeeingRight);
        }
        m_bLastSeeingRight = m_bSeeingRight;
    }

    [PunRPC]
    void ChangeDirectionRPC(bool isSeeingRight)
    {
        Vector3 scale = new Vector3(m_vCharacterObject.transform.localScale.x, m_vCharacterObject.transform.localScale.y, m_vCharacterObject.transform.localScale.z);
        scale.x *= -1;
        m_vCharacterObject.transform.localScale = scale;
        m_vWeaponManager.SetDirection(isSeeingRight);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        /* writing일때 구절과 reading일때 구절은 각 줄이 같은 변수를 대하도록 한다.
        이를테면, 첫번쨰 줄의 stream.SendNext(transform.position)와 curPos = (Vector3)stream.ReceiveNext() 은 같은 위치 좌표에 대한 거고
        두번째 줄의 stream.SendNext(HealthImage.fillAmount)와 HealthImage.fillAmount = (float)stream.ReceiveNext();는 같은 체력바에 대한 것이다. */

        if (stream.IsWriting)    // PV가 IsMine 일 때 작동돼서 넘겨줌
        {
            stream.SendNext(transform.position);        // 캐릭터 위치
            //stream.SendNext(HealthImage.fillAmount);    // 캐릭터 체력
        }
        else                    // IsMine 아닐 때 작동돼서 받음
        {
            m_vCurrentPosition = (Vector3)stream.ReceiveNext();
            //HealthImage.fillAmount = (float)stream.ReceiveNext();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_vPhotonView.IsMine)
        {
            // 땅에 떨어진 무기에 닿으면 해당 무기 획득
            if (collision.tag == "Weapon")
            {
                //Debug.Log(collision.gameObject.GetComponent<WeaponBase>().a_vWeaponData.a_strWeaponName);

                m_vWeaponManager.CheckPickUpWeapon(collision.gameObject);
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = gameObject;
    }
}
