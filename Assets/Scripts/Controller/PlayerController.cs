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
    public PhotonView a_vPhotonView { get { return m_vPhotonView; } }

    [SerializeField] GameObject m_vCharacterObject;
    [SerializeField] Collider2D m_vCharacterMoveCollider;

    CharacterAnimationController m_vCharacterAnimationController;

    [SerializeField] WeaponController m_vWeaponController;
    public WeaponController a_vWeaponController { get { return m_vWeaponController; } }

    [SerializeField] CharacterUIController m_vCharacterUIController;
    public CharacterUIController a_vCharacterUIController { get { return m_vCharacterUIController; } }
    [SerializeField] Collider2D m_vCharacterUIClickCollider;

    [SerializeField] GameObject m_vScoreBoardItemPrefab;
    ScoreBoardItemController m_vScoreBoardItemController;

    private int m_iLastAttackerActorNumber = -1;         // 최근에 플레이어를 공격한 플레이어 id
    private int m_iLastDamagedWeaponID = -1;    // 최근에 플레이어를 공격한 무기 id
    public int a_iLastAttackerActorNumber { get { return m_iLastAttackerActorNumber; } set { m_iLastAttackerActorNumber = value; } }
    public int a_iLastDamagedWeaponID { get { return m_iLastDamagedWeaponID; } set { m_iLastDamagedWeaponID = value; } }

    //*************** Synchronization Properties *******************
    [SerializeField] private int m_iCurrentHealth;
    public int a_iCurrentHealth { get => m_iCurrentHealth; set => SetPropertyRPC(nameof(SetCurrentHealthRPC), value); }
    [PunRPC]
    void SetCurrentHealthRPC(int _iHealth)
    {
        m_iCurrentHealth = _iHealth;
        m_vCharacterUIController.a_iHealth = m_iCurrentHealth;
    }
    //******************************* Player State ********************************
    [SerializeField] private E_PlayerState m_ePlayerState;
    public E_PlayerState a_ePlayerState { get { return m_ePlayerState; } set { SetPropertyRPC(nameof(SetPlayerStateRPC), (int)value); } }
    [PunRPC]
    private void SetPlayerStateRPC(int _iPlayerState)
    {
        m_ePlayerState = (E_PlayerState)_iPlayerState;

        // 플레이어가 죽었으면 유령으로 변한다.
        bool bIsDead = (m_ePlayerState == E_PlayerState.Alive ? false : true);

        m_vCharacterAnimationController.SetGhost(bIsDead);
        m_vCharacterObject.GetComponent<Collider2D>().enabled = !bIsDead;
        m_vCharacterMoveCollider.enabled = !bIsDead;
        m_vCharacterUIClickCollider.enabled = !bIsDead;
        m_vWeaponController.enabled = !bIsDead;

        a_vCharacterUIController.a_ePlayerState = m_ePlayerState;
        m_vScoreBoardItemController.UpdatePlayerState(m_ePlayerState);

        if (m_ePlayerState == E_PlayerState.Missing)
        {
            if (m_vPhotonView.IsMine)
                UIScoreBoardManager.I.UpdateAllPlayerScoreBoard();
        }
        else if (m_ePlayerState == E_PlayerState.Dead)
        {
            m_vScoreBoardItemController.UpdatePlayerRealRole(); // 사망이 확인 된 플레이어의 진짜 직업 공개
        }

        // 살아있는 플레이어는 유령 플레이어를 볼 수 없다.
        if (m_ePlayerState != E_PlayerState.Alive)
        {
            if (GameManager.I.GetPlayerController(PhotonNetwork.LocalPlayer.ActorNumber).m_ePlayerState == E_PlayerState.Alive)
            {
                SetCharacterSprite(false);
                m_vCharacterUIController.SetCanvasBodyActive(false);
            }
        }
        else
        {
            SetCharacterSprite(true);
            if (!m_vPhotonView.IsMine)
            {
                m_vCharacterUIController.SetCanvasBodyActive(false);
            }
        }


    }
    //******************************* Player Role ********************************
    [SerializeField] private E_PlayerRole m_ePlayerRole;
    public E_PlayerRole a_ePlayerRole { get { return m_ePlayerRole; } set { SetPropertyRPC(nameof(SetPlayerRoleRPC), (int)value); } }
    [PunRPC]
    private void SetPlayerRoleRPC(int _iPlayerRole)
    {
        m_ePlayerRole = (E_PlayerRole)_iPlayerRole;

        m_vCharacterUIController.a_iPlayerActorNumber = m_vPhotonView.OwnerActorNr;
        m_vCharacterUIController.SetUIData((m_vPhotonView.Owner.NickName), m_ePlayerRole, m_iCurrentHealth);

        m_vScoreBoardItemController.UpdatePlayerRole(m_ePlayerRole);
    }
    //******************************* Player Ping ********************************
    [SerializeField] private int m_iPlayerPing;
    public int a_iPlayerPing { get { return m_iPlayerPing; } set { SetPropertyRPC(nameof(SetPlayerPingRPC), (int)value); } }
    [PunRPC]
    public void SetPlayerPingRPC(int _iPlayerPing)
    {
        m_iPlayerPing = _iPlayerPing;
        m_vScoreBoardItemController.UpdatePingText(_iPlayerPing);
    }
    //*************** Synchronization Properties End ****************

    void SetPropertyRPC(string functionName, object value)
    {
        m_vPhotonView.RPC(functionName, RpcTarget.All, value);
    }

    public void InvokeProperties()  // Synchronization properites에 새 속성을 넣을경우 여기에 반드시 추가한다.(변수명이 아닌 get set 명임!!)
    {
        a_iCurrentHealth = m_iCurrentHealth;
        a_ePlayerState = m_ePlayerState;
        a_ePlayerRole = m_ePlayerRole;
        a_iPlayerPing = m_iPlayerPing;
    }

    Vector3 m_vCurrentPosition;
    Vector2 m_vTargetPosition;

    bool m_bSeeingRight; // true이면 오른쪽을 보는 상태, false는 왼쪽
    public bool a_bSeeingRight { get { return m_bSeeingRight; } }
    bool m_bLastSeeingRight;

    void Awake()
    {
        GameManager.I.AddPlayerController(m_vPhotonView.OwnerActorNr, this);

        m_vCharacterAnimationController = m_vCharacterObject.GetComponent<CharacterAnimationController>();

        m_vScoreBoardItemController = Instantiate(m_vScoreBoardItemPrefab).GetComponent<ScoreBoardItemController>();
        m_vScoreBoardItemController.InitData(m_vPhotonView.OwnerActorNr, m_vPhotonView.Owner.NickName, m_vPhotonView.Owner.IsMasterClient);

        if (m_vPhotonView.IsMine)
        {
            // 2D 카메라
            CameraManager.I.SetCinemachineCameraFollowAt(transform);
            LightManager.I.SetPlayerLightPosition(transform);
        }

    }

    private void Start()
    {
        //GameManager.I.AddPlayerController(m_vPhotonView.OwnerActorNr, this);

        if (m_vPhotonView.IsMine)
        {
            m_vCharacterUIController.SetCanvasBodyActive(true);

            StartCoroutine(UpdatePingCoroutine());

            m_vTargetPosition = transform.position;
            m_bSeeingRight = true;
            m_bLastSeeingRight = m_bSeeingRight;

            a_ePlayerRole = E_PlayerRole.None;
            // 게임중이거나 쿨링다운이면 관전으로 입장, 준비중이면 생존상태로 입장
            if (GameManager.I.a_eGameState == E_GAMESTATE.Play || GameManager.I.a_eGameState == E_GAMESTATE.Cooling)
            {
                a_ePlayerState = E_PlayerState.Spectator;
                GameManager.I.DisplayGhosts();
                GameManager.I.PlayerNameColorUpdate();
                LightManager.I.DeadPlayerLight();
            }
            else
            {
                a_ePlayerState = E_PlayerState.Alive;
                LightManager.I.AlivePlayerLight();
            }

            //m_vPhotonView.RPC(nameof(InitScoreBoardRPC), RpcTarget.AllBuffered);
        }

        //m_vCharacterController.a_iPlayerActorNumber = m_vPhotonView.OwnerActorNr;
        //m_vCharacterController.SetUIData((m_vPhotonView.Owner.NickName), m_ePlayerRole, m_iCurrentHealth);

    }

    // WaitForSeconds 시간마다 반복해서 유저 핑을 확인하여 점수창에 출력한다.
    private IEnumerator UpdatePingCoroutine()
    {
        a_iPlayerPing = PhotonNetwork.GetPing();

        yield return new WaitForSeconds(4f);

        StartCoroutine(UpdatePingCoroutine());
    }

    private void InitialSetting()   // 처음에는 Serializefield를 통해 에디터에서 값을 줬으므로 Start에선 사용하지않는다.
    {
        a_iCurrentHealth = 100;
    }

    void Update()
    {
        if (m_vPhotonView.IsMine)
        {
            if (!ChatManager.I.a_vInputField.isFocused)
            {
                UpdateWalkingProcess();
                UpdateWeaponAimProcess();
                UpdateWeaponShotProcess();
                UpdateKeyboardInputProcess();
            }
        }

        /* 위치 동기화는 transformView Component를 안 쓰고 OnPhotonSerializeView와 이 코드를 쓰면 빠르고 버그도 없어서 좋다 */
        // IsMine이 아닌 것들은 부드럽게 위치 동기화
        else if ((transform.position - m_vCurrentPosition).sqrMagnitude >= 1) // 너무 멀리 떨어져있으면 바로 순간이동
            transform.position = m_vCurrentPosition;
        else
            transform.position = Vector3.Lerp(transform.position, m_vCurrentPosition, Time.deltaTime * 10); // 적당히 떨어져있으면 부드럽게 이동
    }

    public void Hit(int _idamage, int _iShooterActorNumber, int _iWeaponID)
    {
        a_iCurrentHealth = m_iCurrentHealth - _idamage;
        a_iLastAttackerActorNumber = _iShooterActorNumber;
        a_iLastDamagedWeaponID = _iWeaponID;

        if (m_iCurrentHealth <= 0 && m_ePlayerState == E_PlayerState.Alive)  // 플레이어 사망
        {
            m_ePlayerState = E_PlayerState.Missing;
            a_ePlayerState = E_PlayerState.Missing;

            m_vWeaponController.PlayerDeadProcess();

            float fKillerDistance = GetKillerDistance(_iShooterActorNumber);
            MapManager.I.SpawnPlayerDeadBody(transform.position, m_vPhotonView.Owner.ActorNumber, _iShooterActorNumber, _iWeaponID, PhotonNetwork.Time, fKillerDistance);

            // UIScoreBoardManager.I.UpdateAllPlayerScoreBoard();
            LightManager.I.DeadPlayerLight();
            GameManager.I.PlayerNameColorUpdate();
            GameManager.I.DisplayGhosts();
            GameManager.I.CheckGameOver(m_vPhotonView.OwnerActorNr);

            ChatManager.I.ToggleTeamChat(false);
        }
    }

    private float GetKillerDistance(int _iKillerActorNumber)
    {
        return Vector2.Distance(transform.position, GameManager.I.GetPlayerController(_iKillerActorNumber).transform.position);
    }


    void WeaponRotation(float _fAngle)
    {
        m_vWeaponController.gameObject.transform.rotation = Quaternion.AngleAxis(_fAngle, Vector3.forward);
    }

    void UpdateWeaponShotProcess()
    {
        // 총 발사
        if (Input.GetMouseButton(0))
        {
            m_vWeaponController.Shoot();

        }
        // 발사 중지
        else if (Input.GetMouseButtonUp(0))
        {
            m_vWeaponController.StopShooting();
        }

        // 무기 조준
        if (Input.GetMouseButtonDown(1))
        {
            m_vWeaponController.ToggleAim();
        }
    }

    void UpdateKeyboardInputProcess()
    {
        if (Input.GetKeyDown(KeyCode.R)) // 장전
        {
            m_vWeaponController.Reload();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1))   // 주무기
        {
            m_vWeaponController.ChangeCurrentWeapon(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))  // 보조무기
        {
            m_vWeaponController.ChangeCurrentWeapon(2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))  // 근접무기
        {
            m_vWeaponController.ChangeCurrentWeapon(3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))  // 투척무기
        {
            m_vWeaponController.ChangeCurrentWeapon(4);
        }

        else if (Input.GetKeyDown(KeyCode.G))    // 무기 버리기
        {
            m_vWeaponController.DropWeapon();
        }

        else if (Input.GetKeyDown(KeyCode.Tab))  // 점수창 열기
        {
            UIScoreBoardManager.I.ShowScoreBoard(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))   // 점수창 닫기
        {
            UIScoreBoardManager.I.ShowScoreBoard(false);
        }
    }

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
        Vector2 vMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float fAngle = Mathf.Atan2(vMousePosition.y - m_vTargetPosition.y, vMousePosition.x - m_vTargetPosition.x) * Mathf.Rad2Deg;

        SetDirection(fAngle);

        WeaponRotation(fAngle);
    }

    public void TeleportPlayer(float _fPositionX, float _fPositionY)
    {
        m_vPhotonView.RPC(nameof(TeleportPlayerRPC), RpcTarget.AllViaServer, _fPositionX, _fPositionY);
    }

    [PunRPC]
    private void TeleportPlayerRPC(float _fPositionX, float _fPositionY)
    {
        transform.position = new Vector3(_fPositionX, _fPositionY);
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

    public void SetCharacterSprite(bool _bIsEnabled)
    {
        m_vCharacterObject.GetComponent<SpriteRenderer>().enabled = _bIsEnabled;
        // m_vCharacterUIController.SetCanvasBodyActive(_bIsEnabled);
    }

    [PunRPC]
    void ChangeDirectionRPC(bool isSeeingRight)
    {
        Vector3 scale = new Vector3(m_vCharacterObject.transform.localScale.x, m_vCharacterObject.transform.localScale.y, m_vCharacterObject.transform.localScale.z);
        scale.x *= -1;
        m_vCharacterObject.transform.localScale = scale;
        m_vWeaponController.SetDirection(isSeeingRight);
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
                m_vWeaponController.CheckPickUpWeapon(collision.gameObject.transform.parent.gameObject);
            }
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        info.Sender.TagObject = gameObject;
    }

    // Player Discoonected
    private void OnDestroy()
    {
        //if (!NetworkManager.I.a_bActiveState || m_vPhotonView == null || m_vPhotonView.IsMine)
        //    return;

        //GameManager.I?.RemovePlayerController(m_vPhotonView.OwnerActorNr);
        //UIScoreBoardManager.I?.RemoveScoreBoardItem(m_vPhotonView.OwnerActorNr);

        //if (PhotonNetwork.IsMasterClient)
        //{
        //    ChatManager.I.SendChat(E_ChatType.System, GameManager.I.GetPlayerNickName(m_vPhotonView.OwnerActorNr) + " left the game.");
        //    // 살아있는 플레이어가 나가면 해당 플레이어의 시체를 소환한다.
        //    if (!MapManager.I.a_dicPlayerDead.ContainsKey(m_vPhotonView.OwnerActorNr) && m_ePlayerState == E_PlayerState.Alive)
        //    {
        //        // 시스템상의 자살이나 게임종료로 인한 사망은 ShooterActorNumber, WeaponID, killerDistance 를 전부 0으로 표시한다.
        //        MapManager.I.SpawnPlayerDeadBody(transform.position, m_vPhotonView.Owner.ActorNumber, 0, 0, PhotonNetwork.Time, 0f);
        //        // a_ePlayerState = E_PlayerState.Missing;
        //        GameManager.I.CheckGameOver(m_vPhotonView.OwnerActorNr);
        //    }
        //}
        //if (m_vWeaponController != null)
        //    m_vWeaponController.DropAllWeaponsOnLeft();

    }
}
