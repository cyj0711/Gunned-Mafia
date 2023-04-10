using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawn : MonoBehaviour
{

    public List<GameObject> weaponsToSpawn = new List<GameObject>();

    public GameObject GetWeaponToSpawn()
    {
        if (weaponsToSpawn.Count == 0)
            return null;

        return (weaponsToSpawn[Random.Range(0, weaponsToSpawn.Count)]);
    }
}
