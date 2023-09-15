using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_WeaponType
{
    AssaultRifle,
    Pistol,
    SniperRifle,
    SMG,
    MachineGun,
    Shotgun
}

public enum E_EquipType
{
    Primary,
    Secondary,
    Melee,
    Grenade
}

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    private int m_iWeaponID;
    public int a_iWeaponId { get { return m_iWeaponID; } }

    [SerializeField]
    private string m_strWeaponName;  // 무기 이름
    public string a_strWeaponName { get { return m_strWeaponName; } }

    [SerializeField]
    private int m_iDamage;     // 일반 데미지
    public int a_iDamage { get { return m_iDamage; } }

    [SerializeField]
    private int m_iCriticalDamage;     // 치명타 데미지
    public int a_iCriticalDamage { get { return m_iCriticalDamage; } }

    [SerializeField]
    private float m_fCriticalChance;   // 치명타 확률
    public float a_fCriticalChance { get { return m_fCriticalChance; } }

    [SerializeField]
    private int m_iMaxAmmo;    // 최대 탄창
    public int a_iMaxAmmo { get { return m_iMaxAmmo; } }

    [SerializeField]
    private int m_iAmmoCapacity;    // 한 탄창의 총알 수
    public int a_iAmmoCapacity { get { return m_iAmmoCapacity; } }

    [SerializeField]
    private float m_fReloadTime;    // 장전 시간 (샷건은 한발당 적용)
    public float a_fReloadTime { get { return m_fReloadTime; } }

    [SerializeField]
    private bool m_bAutoFire;    // 자동사격 여부
    public bool a_bAutoFire { get { return m_bAutoFire; } }

    [SerializeField]
    private float m_fRateOfFire;    // 연사 속도(rateOfFire 초당 1발 발사)
    public float a_fRateOfFire { get { return m_fRateOfFire; } }

    [SerializeField]
    private float m_fMaxRecoilAmount;    // 반동의 최대값 (아무리 반동이 심해도 해당 값을 넘길 수 없슴)
    public float a_fMaxRecoilAmount { get { return m_fMaxRecoilAmount; } }

    [SerializeField]
    private float m_fRecoilIncreaseRate;    // 발사 시 반동 증가 수치 (m_fRecoilIncreaseRate 값의 범위만큼 총알의 방향이 랜덤으로 휘어짐)
    public float a_fRecoilIncreaseRate { get { return m_fRecoilIncreaseRate; } }

    [SerializeField]
    private float m_fRecoilDecreaseRate;    // 초당 반동 감소 수치 (1초당 m_fRecoilDecreaseRate 값 만큼 반동을 낮춤)
    public float a_fRecoilDecreaseRate { get { return m_fRecoilDecreaseRate; } }

    [SerializeField]
    private float m_fZoomFactor;    // 조준 했을때 줌의 정도(0f ~ 3f가 최대)
    public float a_fZoomFactor { get { return m_fZoomFactor; } }

    [SerializeField]
    private E_EquipType m_eEquipType;     // 무기를 꺼낼 숫자 키패드.(1 = 주무기, 2 = 보조무기, 3 = 근접무기, 4 = 수류탄)
    public E_EquipType a_eEquipType { get { return m_eEquipType; } }

    [SerializeField]
    private E_WeaponType m_eWeaponType;
    public E_WeaponType a_eWeaponType { get { return m_eWeaponType; } }

    [SerializeField]
    private GameObject m_vWeaponPrefab;
    public GameObject a_vWeaponPrefab { get { return m_vWeaponPrefab; } }

    [SerializeField]
    private GameObject m_vBulletPrefab;
    public GameObject a_vBulletPrefab { get { return m_vBulletPrefab; } }
}
