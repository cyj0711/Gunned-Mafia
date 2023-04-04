using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 
 */

[CreateAssetMenu(fileName = "Weapon Data", menuName = "Scriptable Object/Weapon Data", order = int.MaxValue)]
public class WeaponData : ScriptableObject
{
    [SerializeField]
    private string weaponName;
    public string WeaponName { get { return weaponName; } }
    [SerializeField]
    private int damage;
    public int Damage { get { return damage; } }
}
