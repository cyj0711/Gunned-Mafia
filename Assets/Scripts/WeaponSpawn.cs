using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour
{

    public List<WeaponData> weaponsToSpawn = new List<WeaponData>();

    public WeaponData GetWeaponToSpawn()
    {
        if (weaponsToSpawn.Count == 0)
            return null;

        return (weaponsToSpawn[Random.Range(0, weaponsToSpawn.Count)]);
    }
}
