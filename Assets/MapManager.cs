using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime;
using System;

public class MapManager : SingletonPunCallbacks<MapManager>
{
    public List<WeaponSpawn> weaponSpawnPoints = new List<WeaponSpawn>();

    public void SpawnWeapons()
    {
        foreach(WeaponSpawn weaponSpawnPoint in weaponSpawnPoints)
        {
            GameObject weapon = weaponSpawnPoint.GetWeaponToSpawn();

            if(weapon!=null)
            {
                String weaponName = "WeaponPrefab/" + weapon.GetComponent<WeaponBase>().a_vWeaponData.name;
                PhotonNetwork.InstantiateRoomObject(weaponName, weaponSpawnPoint.transform.position, Quaternion.identity);
            }
        }
    }

    //[PunRPC]
    //void SetGameStateRPC(GameObject weapon, Vector3 spawnPoint)
    //{
    //    Instantiate(weapon, spawnPoint, Quaternion.identity);
    //}
}
