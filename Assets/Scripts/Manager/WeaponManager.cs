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

    [SerializeField]
    GameObject bullet;

    //*************** Synchronization Properties *******************
    [SerializeField] int currentWeaponID;   // 아무것도 안들었을땐 -1 로 설정해주자.
    public int CurrentWeaponID { get => currentWeaponID; set => SetPropertyRPC(nameof(CurrentWeaponIDRPC), value); }
    [PunRPC] void CurrentWeaponIDRPC(int value)
    { 
        currentWeaponID = value;

        if (CurrentWeaponID == -1) { return; }

        GameObject weaponObject = PhotonView.Find(currentWeaponID).gameObject;
        if (currentWeapon != null)
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
    //**************************************************************

    void SetPropertyRPC(string functionName, object value)
    {
        pView.RPC(functionName, RpcTarget.All, value);
    }

    public void InvokeProperties()  // Synchronization properites에 새 속성을 넣을경우 여기에 반드시 추가한다.(변수명이 아닌 get set 명임!!)
    {
        CurrentWeaponID = CurrentWeaponID;
    }


    void Start()
    {
        fireCoolTime = rateOfFire;
        //InitWeaponManager();
    }

    public void InitWeaponManager()
    {
        currentWeapon = null;
        weaponDictionary.Clear();
    }

    void Update()
    {
        CheckCanShooting();

        //Debug.Log(gameObject.transform.eulerAngles.z);
        //if (transform.eulerAngles.z > 90 && transform.eulerAngles.z < 270)
        //    SetDirection(false);
        //else if (transform.eulerAngles.z < 90 || transform.eulerAngles.z > 270)
        //    SetDirection(true);
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
        if(currentWeapon!=null)
        {
            currentWeapon.Shoot();
        }
    }

    public void SpawnProjectile()
    {

        //pView.RPC(nameof(ShootRPC), RpcTarget.All, muzzlePosition.position, Quaternion.Euler(0f, 0f, angle));
    }

    [PunRPC]
    public void ShootRPC(Vector3 position, Quaternion rotation)
    {
        Instantiate(bullet, position, rotation);
    }

    // angle을 통해 유저가 오른쪽을 보는지 왼쪽을 보는지 확인
    public void SetDirection(bool playerSeeingRight)
    {
        isSeeingRight = playerSeeingRight;
        //if(currentWeapon==null)
        //{
        //    if(CurrentWeaponID==-1)
        //    {
        //        return;
        //    }
        //    else
        //    {
        //        currentWeapon = PhotonView.Find(currentWeaponID).gameObject.GetComponent<WeaponBase>();
        //    }
        //}
        if (currentWeapon!=null)
            currentWeapon.gameObject.GetComponent<SpriteRenderer>().flipY = !isSeeingRight;

        //Debug.Log(pView.Owner.NickName +"("+ currentWeapon + (isSeeingRight ? "): Right" : "): Left"));
        // TODO : 게임을 중간에 접속 할 경우, 기존에 있던 유저들의 currentWeapon이 제대로 동기화안돼서 스프라이트가 안뒤집힌다.
    }

    // 플레이어가 땅에 떨어진 무기에 닿으면 해당 무기를 획득한다.
    public void PickUpWeapon(GameObject weaponObject)
    {
        WeaponBase weaponBase = weaponObject.GetComponent<WeaponBase>();
        if(weaponBase==null)
        {
            Debug.Log("Debug ERROR : " + weaponObject + " weaponBase is NULL");
            return;
        }

        // 만약 닿은 무기의 타입을 이미 가지고 있는 경우, 해당 무기를 획득하지 않는다.
        if(weaponDictionary.ContainsKey(weaponBase.GetWeaponData.WeaponType))
        {
            Debug.Log("Tried to pick up [" + weaponBase.GetWeaponData.WeaponName + "], But [" + weaponBase.GetWeaponData.WeaponType + "] type is already equiped!");
        }
        else
        {
            weaponDictionary.Add(weaponBase.GetWeaponData.WeaponType, weaponBase);

            int weaponViewID = weaponObject.GetComponent<PhotonView>().ViewID;  // RPC엔 GameObject를 줄 수 없어서 해당 무기 object의 photon view ID를 대신 준다.
            pView.RPC(nameof(PuckUpWeaponRPC), RpcTarget.AllBuffered, weaponViewID);

            // 현재 아무 무기도 들고있지 않은 상태면, 획득한 무기를 즉시 장착한다.
            if (currentWeapon == null)
            {
                //pView.RPC(nameof(SetCurrentWeaponRPC), RpcTarget.AllBuffered, weaponViewID);
                CurrentWeaponID = weaponViewID;
            }
        }
    }

    // 땅에 떨어진 무기를 플레이어가 가져갔다는 정보를 모든 유저에게 알려준다.
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

    //// 플레이어가 현재 들고 있는 무기를 세팅한다.
    //[PunRPC]
    //private void SetCurrentWeaponRPC(int weaponViewID)
    //{
    //    GameObject weaponObject = PhotonView.Find(weaponViewID).gameObject;
    //    if (currentWeapon!=null)
    //    {
    //        currentWeapon.gameObject.SetActive(false);
    //    }

    //    currentWeapon = weaponObject.GetComponent<WeaponBase>();
    //    if (currentWeapon == null)
    //    {
    //        Debug.Log("Debug ERROR : current " + weaponObject + " weaponBase is NULL");
    //        return;
    //    }

    //    SetDirection(isSeeingRight);
    //    weaponObject.SetActive(true);

    //}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
