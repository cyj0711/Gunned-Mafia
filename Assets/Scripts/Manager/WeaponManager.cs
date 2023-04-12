using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class WeaponManager : MonoBehaviourPunCallbacks , IPunObservable
{
    public PhotonView pView;

    public List<GameObject> weapons = new List<GameObject>();

    private Dictionary<E_WeaponType, WeaponBase> weaponDictionary = new Dictionary<E_WeaponType, WeaponBase>();

    public Transform muzzlePosition;

    private WeaponBase currentWeapon;

    private float rateOfFire = 0.1f;
    float fireCoolTime;

    private bool canShooting = true;

    private bool isSeeingRight = true;


    void Start()
    {
        fireCoolTime = rateOfFire;
        InitWeaponManager();
    }

    public void InitWeaponManager()
    {
        currentWeapon = null;
        weaponDictionary.Clear();
    }

    void Update()
    {
        CheckCanShooting();

    }

    // 연사속도를 통해 연사 조절
    private void CheckCanShooting()
    {
        if (canShooting == false)
        {
            if (fireCoolTime <= 0f)
            {
                canShooting = true;
                fireCoolTime = rateOfFire;
            }
            else
            {
                fireCoolTime -= Time.deltaTime;
            }
        }
    }

    // 플레이어가 마우스를 누르면 이 함수가 호출되어 총알 발사
    public void Shoot(float angle)
    {
        if (canShooting)
        {
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
            /* 꿀팁 
            PhotonNetwork.Instantiate("Bullet", muzzlePosition.position, Quaternion.Euler(0f, 0f, angle))
                .GetComponent<PhotonView>().RPC("RPCfunction",RpcTarget,RPCparameter)
            를 쓰면 instantiate 한 오브젝트의 rpc를 호출할 수 있다.
            */
            canShooting = false;
        }
    }


    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    public void SetDirection(bool playerSeeingRight)
    {
        //for(int i=0;i<weapons.Count;i++)
        //{
        //    weapons[i].GetComponent<SpriteRenderer>().flipY = !isSeeingRight;
        //}
        isSeeingRight = playerSeeingRight;
        if (currentWeapon!=null)
            currentWeapon.gameObject.GetComponent<SpriteRenderer>().flipY = !isSeeingRight;
    }

    public void PickUpWeapon(GameObject weaponObject)
    {
        WeaponBase weaponBase = weaponObject.GetComponent<WeaponBase>();
        if(weaponBase==null)
        {
            Debug.Log("Debug ERROR : " + weaponObject + " weaponBase is NULL");
            return;
        }

        if(weaponDictionary.ContainsKey(weaponBase.GetWeaponData.WeaponType))
        {
            Debug.Log("Tried to pick up [" + weaponBase.GetWeaponData.WeaponName + "], But [" + weaponBase.GetWeaponData.WeaponType + "] type is already equiped!");
        }
        else
        {
            weaponDictionary.Add(weaponBase.GetWeaponData.WeaponType, weaponBase);

            int weaponViewID = weaponObject.GetComponent<PhotonView>().ViewID;
            pView.RPC("PuckUpWeaponRPC", RpcTarget.AllBuffered, weaponViewID);

            if (currentWeapon == null)
            {
                pView.RPC("SetCurrentWeaponRPC", RpcTarget.AllBuffered, weaponViewID);
            }
        }
    }

    [PunRPC]
    private void PuckUpWeaponRPC(int weaponViewID)
    {
        GameObject weaponObject = PhotonView.Find(weaponViewID).gameObject;

        weaponObject.GetComponent<CapsuleCollider2D>().enabled = false;

        weaponObject.transform.parent = gameObject.transform;
        weaponObject.transform.localPosition = new Vector3(0f, 0f, 0f);
        weaponObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        weaponObject.SetActive(false);
    }


    [PunRPC]
    private void SetCurrentWeaponRPC(int weaponViewID)
    {
        GameObject weaponObject = PhotonView.Find(weaponViewID).gameObject;
        if (currentWeapon!=null)
        {
            currentWeapon.gameObject.SetActive(false);
        }

        currentWeapon = weaponObject.GetComponent<WeaponBase>();
        if (currentWeapon == null)
        {
            Debug.Log("Debug ERROR : current " + weaponObject + " weaponBase is NULL");
            return;
        }

        SetDirection(isSeeingRight);
        weaponObject.SetActive(true);

    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
