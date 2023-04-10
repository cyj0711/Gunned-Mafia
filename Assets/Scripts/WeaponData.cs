using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_BulletType
{
    Pistol,
    SniperRifle,
    SMG,
    MachineGun,
    Shotgun,
    Magnum
}

public enum E_WeaponType
{
    Primary,
    Sub,
    Melee,
    Grenade
}

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    private string weaponName;  // 무기 이름
    public string WeaponName { get { return weaponName; } }

    [SerializeField]
    private int damage;     // 일반 데미지
    public int Damage { get { return damage; } }

    [SerializeField]
    private int criticalDamage;     // 치명타 데미지
    public int CriticalDamage { get { return criticalDamage; } }

    [SerializeField]
    private float criticalChance;   // 치명타 확률
    public float CriticalChance { get { return criticalChance; } }

    [SerializeField]
    private int maxAmmo;    // 최대 탄창
    public int MaxAmmo { get { return maxAmmo; } }

    [SerializeField]
    private int ammoInMagazine;    // 한 탄창의 총알 수
    public int AmmoInMagazine { get { return ammoInMagazine; } }

    [SerializeField]
    private float reloadTime;    // 장전 시간 (샷건은 한발당 적용)
    public float ReloadTime { get { return reloadTime; } }

    [SerializeField]
    private bool isAutoFire;    // 자동사격 여부
    public bool IsAutoFire { get { return isAutoFire; } }

    [SerializeField]
    private E_WeaponType weaponType;     // 무기를 꺼낼 숫자 키패드.(1 = 주무기, 2 = 보조무기, 3 = 근접무기, 4 = 수류탄)
    public E_WeaponType WeaponType { get { return weaponType; } }

    [SerializeField]
    private E_BulletType bulleyType;
    public E_BulletType BulleyType { get { return bulleyType; } }

    [SerializeField]
    private GameObject weaponPrefab;
    public GameObject WeaponPrefab { get { return weaponPrefab; } }
}
