using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputManager : Singleton<MobileInputManager>//, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] GameObject m_vMobilePanelObject;

    [SerializeField] RectTransform m_vLeftJoystick;
    [SerializeField] RectTransform m_vLeftLever;

    [SerializeField] RectTransform m_vRightJoystick;
    [SerializeField] RectTransform m_vRightLever;

    [SerializeField] RectTransform m_vChatPanelTransform;

    RectTransform m_vCurrentJoystick;
    RectTransform m_vCurrentLever;

    private bool m_bIsLeftTouched = false;
    private bool m_bIsRightTouched = false;

    private int m_iLeftID = -1;
    private int m_iRightID = -1;

    private float m_fLeverMovementLimit;

    private Vector2 vMouseDownPosition;
    private Vector2 vMouseMovePosition;

    private Vector2 vLeftTouchDownPosition;
    private Vector2 vRightTouchDownPosition;

    private int iLayerTouchable;

    private PlayerController m_vLocalPlayer;

    void Start()
    {
        if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
            gameObject.SetActive(false);
        else
        {
            m_vMobilePanelObject.SetActive(true);
            m_vChatPanelTransform.anchorMin = new Vector2(0f, 1f);
            m_vChatPanelTransform.anchorMax = new Vector2(0f, 1f);
            m_vChatPanelTransform.anchoredPosition = new Vector3(m_vChatPanelTransform.anchoredPosition.x, -100f, 0f);
        }

        m_fLeverMovementLimit = (m_vLeftJoystick.rect.width - m_vLeftLever.GetComponent<RectTransform>().rect.width) / 2;
        iLayerTouchable = 1 << LayerMask.NameToLayer("Touchable");

        m_vLocalPlayer = GameManager.I.GetPlayerController();
    }

    void Update()
    {
        if (m_vLocalPlayer == null) return;
        //OnMultiTouch();
        //MouseTouch();
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

                if (eTouchPhase == TouchPhase.Began)
                {
                    // 터치한곳에 상호작용 오브젝트가 있으면 조이스틱을 생성하지않고 해당 상호작용을 진행함
                    RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, iLayerTouchable);
                    if (hit.collider != null)
                    {
                        return;
                    }

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
                        }
                    }
                }
                else if (eTouchPhase == TouchPhase.Moved || eTouchPhase == TouchPhase.Stationary)
                {
                    if (iTouchID == m_iLeftID) // left touching
                    {
                        Vector2 vLeverPos = vTouchPosition - vLeftTouchDownPosition;

                        if (Vector3.Distance(vTouchPosition, vLeftTouchDownPosition) > m_fLeverMovementLimit)
                        {
                            vLeverPos = (vTouchPosition - vLeftTouchDownPosition).normalized * m_fLeverMovementLimit;
                        }

                        m_vLeftLever.localPosition = vLeverPos;

                        m_vLocalPlayer.SetWalkingInput(vLeverPos.x, vLeverPos.y);
                    }
                    else                    // right touching
                    {
                        Vector2 vLeverPos = vTouchPosition - vRightTouchDownPosition;

                        if (Vector3.Distance(vTouchPosition, vRightTouchDownPosition) > m_fLeverMovementLimit)
                        {
                            vLeverPos = (vTouchPosition - vRightTouchDownPosition).normalized * m_fLeverMovementLimit;
                        }

                        m_vRightLever.localPosition = vLeverPos;

                        // 레버를 움직였을때 총을 발사함
                        if(m_vRightLever.localPosition!=Vector3.zero)
                        {
                            Vector3 m_vTargetPosition = Vector3.zero;
                            Vector3 vMousePosition = m_vRightLever.localPosition;
                            float fAngle = Mathf.Atan2(vMousePosition.y - m_vTargetPosition.y, vMousePosition.x - m_vTargetPosition.x) * Mathf.Rad2Deg;

                            m_vLocalPlayer.SetDirection(fAngle);
                            m_vLocalPlayer.WeaponRotation(fAngle);
                            m_vLocalPlayer.a_vWeaponController.Shoot();
                        }
                    }
                }
                else if (eTouchPhase == TouchPhase.Ended || eTouchPhase == TouchPhase.Canceled)
                {
                    if (iTouchID == m_iLeftID) // left touch end
                    {
                        m_iLeftID = -1;

                        m_vLeftLever.localPosition = Vector3.zero;
                        m_vLeftJoystick.gameObject.SetActive(false);

                        m_vLocalPlayer.SetWalkingInput(0f, 0f);
                    }
                    else                    // right touch end
                    {
                        m_iRightID = -1;

                        m_vRightLever.localPosition = Vector3.zero;
                        m_vRightJoystick.gameObject.SetActive(false);

                        m_vLocalPlayer.a_vWeaponController.StopShooting();
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
        if(m_vLocalPlayer.a_vWeaponController.a_vCurrentWeapon.a_vWeaponData.a_eEquipType==E_EquipType.Primary)

            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(2);
        else

            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(1);
    }

    private void MouseTouch()
    {
        // 조이스틱 on
        if (Input.GetMouseButtonDown(0))
        {
            // 터치한곳에 상호작용 오브젝트가 있으면 조이스틱을 생성하지않고 해당 상호작용을 진행함
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, iLayerTouchable);
            if (hit.collider != null)
            {
                return;
            }

            Vector3 vMoustPos = Input.mousePosition;
            vMouseDownPosition = Input.mousePosition;

            if (vMoustPos.x < Screen.width / 2) // Left Touch
            {
                if (!m_bIsLeftTouched)
                {
                    m_bIsLeftTouched = true;

                    Vector3 vJoystickPos = Camera.main.ScreenToWorldPoint(vMoustPos);
                    vJoystickPos.z = 0f;

                    m_vLeftJoystick.transform.position = vJoystickPos;
                    m_vLeftJoystick.gameObject.SetActive(true);

                    m_vCurrentJoystick = m_vLeftJoystick;
                    m_vCurrentLever = m_vLeftLever;
                }
                else
                {

                }
            }
            else    // right touch
            {
                if (!m_bIsRightTouched)
                {
                    m_bIsRightTouched = true;

                    Vector3 vJoystickPos = Camera.main.ScreenToWorldPoint(vMoustPos);
                    vJoystickPos.z = 0f;

                    m_vRightJoystick.transform.position = vJoystickPos;
                    m_vRightJoystick.gameObject.SetActive(true);

                    m_vCurrentJoystick = m_vRightJoystick;
                    m_vCurrentLever = m_vRightLever;
                }
                else
                {

                }
            }
        }
        // 조이스틱 레버 이동
        else if (Input.GetMouseButton(0))
        {
            vMouseMovePosition = Input.mousePosition;

            Vector2 vJoystickPos = vMouseMovePosition - vMouseDownPosition;

            //Debug.Log("Mouse: "+ vMoustPos.ToString() + " / Stick: " + m_vCurrentJoystick.anchoredPosition.ToString() + " / " + Vector3.Distance(vMoustPos, m_vCurrentJoystick.anchoredPosition).ToString());
            if (Vector3.Distance(vMouseMovePosition, vMouseDownPosition) > m_fLeverMovementLimit)
            {
                vJoystickPos = (vMouseMovePosition - vMouseDownPosition).normalized * m_fLeverMovementLimit;
            }

            m_vCurrentLever.localPosition = vJoystickPos;
        }
        // 조이스틱 off
        else if (Input.GetMouseButtonUp(0))
        {
            m_bIsLeftTouched = false;
            m_bIsRightTouched = false;

            m_vCurrentLever.localPosition = Vector3.zero;
            m_vCurrentJoystick.gameObject.SetActive(false);

            m_vCurrentJoystick = null;
            m_vCurrentLever = null;
        }


    }

    //private void OnMultiTouch()
    //{
    //    if(Input.touchCount>0)
    //    {
    //        for(int i=0;i<Input.touchCount;i++)
    //        {
    //            Touch touch = Input.GetTouch(i);
    //            int index = touch.fingerId;
    //            Vector2 position = touch.position;
    //            TouchPhase phase = touch.phase;

    //            if (phase == TouchPhase.Began)
    //            {
    //                if(touch.position.x<Screen.width/2) // Left Touch
    //                {
    //                    if(!m_bIsLeftTouched)
    //                    {
    //                        m_bIsLeftTouched = true;

    //                        m_vLeftJoystick.gameObject.SetActive(true);
    //                    }
    //                    else
    //                    {

    //                    }
    //                }
    //                else   // Right Touch
    //                {

    //                }
    //            }
    //            else if (phase == TouchPhase.Moved)
    //            {

    //            }
    //            else if (phase == TouchPhase.Stationary)
    //            {

    //            }
    //            else if (phase == TouchPhase.Ended)
    //            {

    //            }
    //            else if(phase==TouchPhase.Canceled)
    //            {

    //            }
    //        }
    //    }
    //}

    //public void OnBeginDrag(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void OnDrag(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}

    //public void OnEndDrag(PointerEventData eventData)
    //{
    //    throw new System.NotImplementedException();
    //}

}
