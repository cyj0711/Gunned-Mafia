using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DesktopInputManager : Singleton<DesktopInputManager>
{
    private PlayerController m_vLocalPlayer;
    private int iLayerTouchable;

    void Start()
    {
        // 모바일 구동시 DesktopInputManager 비활성화.
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
            gameObject.SetActive(false);

        m_vLocalPlayer = GameManager.I.GetPlayerController();
        iLayerTouchable = 1 << LayerMask.NameToLayer("Touchable");
    }

    void Update()
    {
        if (!ChatManager.I.a_vInputField.isFocused)
        {
            UpdateWalkingProcess();
            UpdateWeaponAimProcess();
            UpdateWeaponShotProcess();
            UpdateKeyboardInputProcess();
        }
    }

    void UpdateWeaponShotProcess()
    {
        // 총 발사
        if (Input.GetMouseButton(0))
        {
            //Debug.Log(Input.mousePosition);

            // 클릭한곳에 버튼 등 상호작용 오브젝트가 있으면 총을 쏘지않고 해당 상호작용을 진행함
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, Mathf.Infinity, iLayerTouchable);
            if (hit.collider != null)
            {
                return;
            }
            //if (EventSystem.current.IsPointerOverGameObject())
            //{
            //    return;
            //}

            m_vLocalPlayer.a_vWeaponController.Shoot();

        }
        // 발사 중지
        else if (Input.GetMouseButtonUp(0))
        {
            m_vLocalPlayer.a_vWeaponController.StopShooting();
        }

        // 무기 조준
        if (Input.GetMouseButtonDown(1))
        {
            m_vLocalPlayer.a_vWeaponController.ToggleAim();
        }
    }

    void UpdateKeyboardInputProcess()
    {
        if (Input.GetKeyDown(KeyCode.R)) // 장전
        {
            m_vLocalPlayer.a_vWeaponController.Reload();
        }

        else if (Input.GetKeyDown(KeyCode.Alpha1))   // 주무기
        {
            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Primary);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))  // 보조무기
        {
            m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Secondary);
        }
        //else if (Input.GetKeyDown(KeyCode.Alpha3))  // 근접무기
        //{
        //    m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Melee);
        //}
        //else if (Input.GetKeyDown(KeyCode.Alpha4))  // 투척무기
        //{
        //    m_vLocalPlayer.a_vWeaponController.ChangeCurrentWeapon(E_EquipType.Grenade);
        //}

        else if (Input.GetKeyDown(KeyCode.G))    // 무기 버리기
        {
            m_vLocalPlayer.a_vWeaponController.DropWeapon();
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
        m_vLocalPlayer.SetWalkingInput(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    void UpdateWeaponAimProcess()
    {
        Vector3 m_vTargetPosition = m_vLocalPlayer.transform.position;
        Vector2 vMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float fAngle = Mathf.Atan2(vMousePosition.y - m_vTargetPosition.y, vMousePosition.x - m_vTargetPosition.x) * Mathf.Rad2Deg;

        m_vLocalPlayer.SetDirection(fAngle);

        m_vLocalPlayer.WeaponRotation(fAngle);
    }
}
