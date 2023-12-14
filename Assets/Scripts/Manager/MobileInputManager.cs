using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MobileInputManager : Singleton<MobileInputManager>//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] GameObject m_vMobilePanelObject;

    [SerializeField] RectTransform m_vLeftJoystick;
    [SerializeField] RectTransform m_vLeftLever;

    [SerializeField] RectTransform m_vRightJoystick;
    public RectTransform a_vRightJoystick { get => m_vRightJoystick; }
    [SerializeField] RectTransform m_vRightLever;
    public RectTransform a_vRightLever { get => m_vRightLever; }

    [SerializeField] RectTransform m_vChatPanelTransform;

    [SerializeField] Image m_vFireButtonImage;
    [SerializeField] Image m_vAimLockButtonImage;

    private int m_iLeftID = -1;
    private int m_iRightID = -1;

    private float m_fLeverMovementLimit;
    public float a_fLeverMovementLimit { get => m_fLeverMovementLimit; }

    private Vector2 vLeftTouchDownPosition;
    private Vector2 vRightTouchDownPosition;

    private int iLayerTouchable;

    private PlayerController m_vLocalPlayer;

    private bool m_bIsFireMode = false;
    private bool m_bIsAimLockMode = false;
    public bool a_bIsAimLockMode { get => m_bIsAimLockMode; }
    private float m_fLeverMoveDistance;
    public float a_fLeverMoveDistance { get => m_fLeverMoveDistance; }
    private Vector3 m_vAimVector;   // 조준 상태를 고정할때 사용하기위해 RightLever vector 대신 사용한다.
    public Vector3 a_vAimVector{ get => m_vAimVector; }

    void Start()
    {
        // 모바일 구동이 아닐 시
        if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
            gameObject.SetActive(false);
        // 모바일 구동 시
        else
        {
            m_vMobilePanelObject.SetActive(true);
            m_vChatPanelTransform.anchorMin = new Vector2(0f, 1f);
            m_vChatPanelTransform.anchorMax = new Vector2(0f, 1f);
            m_vChatPanelTransform.anchoredPosition = new Vector3(m_vChatPanelTransform.anchoredPosition.x, -100f, 0f);
            ChatManager.I.SetChatUIForMobile();
        }

        m_fLeverMovementLimit = (m_vLeftJoystick.rect.width - m_vLeftLever.rect.width) / 2;
        iLayerTouchable = LayerMask.GetMask("Touchable","MouseEvent");

        m_vLocalPlayer = GameManager.I.GetPlayerController();
    }

    void Update()
    {
        if (m_vLocalPlayer == null) return;

        MoblieTouch();
    }

    public void SetLocalPlayer(PlayerController _vLocalPlayer)
    {
        m_vLocalPlayer = _vLocalPlayer;
    }

    private void MoblieTouch()
    {
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                int iTouchID = touch.fingerId;
                Vector2 vTouchPosition = touch.position;
                TouchPhase eTouchPhase = touch.phase;

                if (eTouchPhase == TouchPhase.Began)    // { 조이스틱 터치 시작 }
                {
                    // 터치한곳에 상호작용 오브젝트가 있으면 조이스틱을 생성하지않고 해당 상호작용을 진행함
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(vTouchPosition), Vector2.zero, Mathf.Infinity, iLayerTouchable);
                    if (hit.collider != null)
                    {
                        if (hit.collider.CompareTag("Player"))
                        {
                            CharacterUIController vUIController = hit.collider.GetComponentInParent<CharacterUIController>();
                            if (vUIController != null)
                            {
                                // 플레이어 시야에 없는 플레이어는 눌러도 ui가 안뜨고 레버를 생성함
                                if (!vUIController.CheckIsPlayerOutOfSight())
                                {
                                    vUIController.SetCharacterUIMobile();
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                    // Debug.Log(hit.collider);

                    if (vTouchPosition.x < Screen.width / 2) // Left Touch begin
                    {
                        if (m_iLeftID == -1)
                        {
                            m_iLeftID = iTouchID;

                            Vector3 vJoystickPos = Camera.main.ScreenToWorldPoint(vTouchPosition);
                            vJoystickPos.z = 0f;

                            m_vLeftJoystick.transform.position = vJoystickPos;
                            m_vLeftJoystick.gameObject.SetActive(true);

                            vLeftTouchDownPosition = vTouchPosition;
                        }
                    }
                    else                                    // right touch begin
                    {
                        if (m_iRightID == -1)
                        {
                            m_iRightID = iTouchID;

                            Vector3 vJoystickPos = Camera.main.ScreenToWorldPoint(vTouchPosition);
                            vJoystickPos.z = 0f;

                            m_vRightJoystick.transform.position = vJoystickPos;
                            m_vRightJoystick.gameObject.SetActive(true);

                            vRightTouchDownPosition = vTouchPosition;

                            m_vLocalPlayer.a_vWeaponController.ToggleAim(true);
                        }
                    }
                }
                else if (eTouchPhase == TouchPhase.Moved || eTouchPhase == TouchPhase.Stationary) // { 조이스틱 터치 유지 및 이동 }
                {
                    // 터치한곳에 상호작용 오브젝트가 있으면 조이스틱을 생성하지않고 해당 상호작용을 진행함
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(vTouchPosition), Vector2.zero, Mathf.Infinity, iLayerTouchable);
                    if (hit.collider != null) continue;

                    if (iTouchID == m_iLeftID)          // left touching
                    {
                        Vector2 vLeverPos = vTouchPosition - vLeftTouchDownPosition;

                        if (Vector3.Distance(vTouchPosition, vLeftTouchDownPosition) > m_fLeverMovementLimit)
                        {
                            vLeverPos = (vTouchPosition - vLeftTouchDownPosition).normalized * m_fLeverMovementLimit;
                        }

                        m_vLeftLever.localPosition = vLeverPos;

                        m_vLocalPlayer.SetWalkingInput(vLeverPos.x, vLeverPos.y);
                    }
                    else if (iTouchID == m_iRightID)    // right touching
                    {
                        Vector2 vLeverPos = vTouchPosition - vRightTouchDownPosition;

                        if (Vector3.Distance(vTouchPosition, vRightTouchDownPosition) > m_fLeverMovementLimit)
                        {
                            vLeverPos = (vTouchPosition - vRightTouchDownPosition).normalized * m_fLeverMovementLimit;
                        }

                        m_vRightLever.localPosition = vLeverPos;
                        if(vLeverPos!=Vector2.zero)
                            m_vAimVector = vLeverPos;

                        // 레버를 움직였을때 총을 조준함
                        if (m_vRightLever.localPosition!=Vector3.zero)
                        {
                            //Vector3 vTargetPosition = Vector3.zero;
                            //Vector3 vLeverPosition = m_vRightLever.localPosition;
                            // float fAngle = Mathf.Atan2(vLeverPosition.y - vTargetPosition.y, vLeverPosition.x - vTargetPosition.x) * Mathf.Rad2Deg;
                            float fAngle = Mathf.Atan2(m_vRightLever.localPosition.y, m_vRightLever.localPosition.x) * Mathf.Rad2Deg;

                            m_vLocalPlayer.SetDirection(fAngle);
                            m_vLocalPlayer.WeaponRotation(fAngle);

                            // 발사 토글이 활성화 된 상태에만 총을 쏨.
                            if(m_bIsFireMode)
                                m_vLocalPlayer.a_vWeaponController.Shoot();
                        }
                    }
                }
                else if (eTouchPhase == TouchPhase.Ended || eTouchPhase == TouchPhase.Canceled) //  { 조이스틱 끝 }
                {
                    if (iTouchID == m_iLeftID)          // left touch end
                    {
                        m_iLeftID = -1;

                        m_vLeftLever.localPosition = Vector3.zero;
                        m_vLeftJoystick.gameObject.SetActive(false);

                        m_vLocalPlayer.SetWalkingInput(0f, 0f);
                    }
                    else if (iTouchID == m_iRightID)    // right touch end
                    {
                        m_iRightID = -1;

                        m_vRightLever.localPosition = Vector3.zero;
                        m_vRightJoystick.gameObject.SetActive(false);

                        m_vLocalPlayer.a_vWeaponController.StopShooting();

                        Debug.Log("Aim Lock EndTouch : " + m_bIsAimLockMode);
                        // 조준 고정 상태가 아닐때만 조준을 해제한다.
                        if (!m_bIsAimLockMode)
                        {
                            m_vAimVector = Vector3.zero;
                            m_vLocalPlayer.a_vWeaponController.ToggleAim(false);
                            Debug.Log("End TOuch exe");
                        }
                    }
                }
            }
        }
    }

    public void OnTouchReload()
    {
        m_vLocalPlayer.a_vWeaponController.Reload();
    }

    public void OnTouchDrop()
    {
        m_vLocalPlayer.a_vWeaponController.DropWeapon();
    }

    public void OnTouchSwitch()
    {
        // 현재 들고있는 무기가 없다면 인벤토리에 가진 무기중 하나로 바꾼다(주무기 우선)
        if (m_vLocalPlayer.a_vWeaponController.a_vCurrentWeapon == null)
        {
            if (m_vLocalPlayer.a_vWeaponController.CheckCanEquipWeaponType(E_EquipType.Primary))
                m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Primary);
            else if (m_vLocalPlayer.a_vWeaponController.CheckCanEquipWeaponType(E_EquipType.Secondary))
                m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Secondary);
        }
        else if (m_vLocalPlayer.a_vWeaponController.a_vCurrentWeapon.a_vWeaponData.a_eEquipType == E_EquipType.Primary)
            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Secondary);
        else if(m_vLocalPlayer.a_vWeaponController.a_vCurrentWeapon.a_vWeaponData.a_eEquipType == E_EquipType.Secondary)
            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Primary);
    }

    public void OnTouchFire()
    {
        m_bIsFireMode = !m_bIsFireMode;

        m_vFireButtonImage.color = new Color(1f, (m_bIsFireMode ? 0.6f : 1f), (m_bIsFireMode ? 0f : 1f));
    }

    public void OnTouchAimLock()
    {
        m_bIsAimLockMode = !m_bIsAimLockMode;

        m_vAimLockButtonImage.color = new Color(1f, (m_bIsAimLockMode ? 0.6f : 1f), (m_bIsAimLockMode ? 0f : 1f));

        Debug.Log("Aim Lock : " + m_bIsAimLockMode);

        if(m_bIsAimLockMode)
        {
            m_fLeverMoveDistance = m_vRightLever.localPosition.magnitude;
            Debug.Log("Aim Lock true exe");

        }
        else
        {
            // 조준 고정을 해제할때 오른쪽 레버도 사용하지 않는 상태이면(조준중이지 않으면) 조준을 해제함
            if(m_iRightID==-1)
                m_vLocalPlayer.a_vWeaponController.ToggleAim(false);
            Debug.Log("Aim Lock false exe");

        }
    }

    public void OnTouchSearch()
    {
        UISearchManager.I.SearchBody();
    }

    public void OnTouchScoreboard()
    {
        UIScoreBoardManager.I.ShowScoreBoard();
    }
}
