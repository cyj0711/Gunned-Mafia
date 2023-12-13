using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class LogInManager : SingletonPunCallbacks<LogInManager>
{
    [SerializeField] private Text connectionInfoText;
    [SerializeField] private Text GPGSInfoText;
    [SerializeField] private Text FirebaseInfoText;

    FirebaseAuth fbAuth;

    int m_iFlagMobile = 0;

    private void Start()
    {
        //Screen.SetResolution(960, 540, false);

#if UNITY_ANDROID
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
#endif

        // 이거 쓰면 더 빨라진다는데 정확힌 모름
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        PhotonNetwork.ConnectUsingSettings();   // 여기서 connect가 완료되면 OnConnectedToMaster() 함수가 자동으로 호출된다.

        connectionInfoText.text = "Connecting to Master Server...";
    }

    private void Update()
    {
        if(m_iFlagMobile==1)
        {
            Debug.Log("Flag is on");
            FirebaseInfoText.text = "Firebase Success";
            SceneManager.LoadScene("Lobby");
        }
    }

    public override void OnConnectedToMaster()
    {
        //joinButton.interactable = true;
        connectionInfoText.text = "Online : Connected to Master Server";

        if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
            SceneManager.LoadScene("Lobby");
        else
        {
            SceneManager.LoadScene("Lobby");    // TODO: 블루스택 구동용 코드이므로 실제 빌드에선 반드시 지우고 ConnectGoogle(); 를 활성화해야함
            // ConnectGoogle();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        connectionInfoText.text = $"Offline : Connection Disabled {cause.ToString()} - Try reconnecting...";

        PhotonNetwork.ConnectUsingSettings();   // 접속에 실패해도 재접속 시도
    }


    public void ConnectGoogle()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder()
            .RequestIdToken()
            .RequestEmail()
            .Build());
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        fbAuth = FirebaseAuth.DefaultInstance;

        TryGoogleLogin();
#endif
    }

#if UNITY_ANDROID
    public void TryGoogleLogin()
    {
        PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptAlways, (success) =>
        {
            if (success == SignInStatus.Success)
            {
                GPGSInfoText.text = "Google Success";
                Debug.Log("1");
                StartCoroutine(TryFirebaseLogin());
            }
            else
            {
                GPGSInfoText.text = "Google Failure";
            }
        });

        //Social.localUser.Authenticate((bool success) =>
        //{
        //    if (success)
        //    {
        //        GPGSInfoText.text = Social.localUser.id + " : " + Social.localUser.userName;
        //        //StartCoroutine(TryFirebaseLogin());
        //    }
        //    else
        //        GPGSInfoText.text = "Failed to Log in to Google.";
        //});

        //Social.localUser.Authenticate(sucess =>
        //{
        //    if (sucess)
        //    {
        //        GPGSInfoText.text = Social.localUser.id + " : " + Social.localUser.userName;
        //        int loginValue = FirebaseLogIn(PlayGamesPlatform.Instance.GetServerAuthCode());
        //    }
        //});
    }
#endif
//public int FirebaseLogIn(string authcode)
//{
//    Debug.Log("authcode : " + authcode);
//    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

//    while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken())) { }
//    Debug.Log("1");

//    authcode= ((PlayGamesLocalUser)Social.localUser).GetIdToken();
//    Debug.Log("authcode : " + authcode);

//    Firebase.Auth.Credential credential =
//        Firebase.Auth.GoogleAuthProvider.GetCredential(authcode, null);

//    Debug.Log("2");

//    auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
//    {
//        Debug.Log("3");

//        if(task==null)
//        {
//            Debug.Log("3.1");
//            return -1;
//        }

//        if (task.IsCanceled)
//        {
//            Debug.Log("3.2");
//            Debug.LogError("SignInWithCredentialAsync was canceled.");
//            return -2;
//        }

//        if (task.IsFaulted)
//        {
//            Debug.Log("3.3");
//            Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
//            return -3;
//        }
//        Debug.Log("4");

//        Firebase.Auth.FirebaseUser newUser = task.Result;
//        Debug.LogFormat("User signed in successfully: {0} ({1})",
//            newUser.DisplayName, newUser.UserId);

//        Debug.Log("5");
//        return 1;
//    });
//    Debug.Log("6");
//}

#if UNITY_ANDROID
    public void TryGoogleLogout()
    {
        if (Social.localUser.authenticated)
        {
            ((PlayGamesPlatform)Social.Active).SignOut();
            fbAuth.SignOut();
        }
    }


    IEnumerator TryFirebaseLogin()
    {
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
        {
            yield return null;
        }

        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        Debug.Log("2");

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);

        Debug.Log("3");
        
        fbAuth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            Debug.Log("4");
            if (task.IsCanceled)
            {
                Debug.Log("4.1");
                FirebaseInfoText.text = "Firebase Canceled";
            }
            else if (task.IsFaulted)
            {
                Debug.Log("4.2");
                FirebaseInfoText.text = "Firebase Faulted";
            }
            else
            {
                Debug.Log("4.3");
                //FirebaseInfoText.text = "Firebase Success";
                Debug.Log("4.4");
                //SceneManager.LoadSceneAsync("Lobby");
                //LoadSceneWithName("Lobby");
                m_iFlagMobile = 1;
                Debug.Log("4.5");
            }
            Debug.Log("5");
        });

        Debug.Log("6");
    }
#endif

    public void LoadSceneWithName(string _strSceneName)
    {
        Debug.Log("7");
        SceneManager.LoadScene("Lobby");
    }

    //public void Connect()
    //{
    //    joinButton.interactable = false;    // 중복 접속 시도 차단

    //    if (PhotonNetwork.IsConnected)
    //    {
    //        connectionInfoText.text = "Connecting to Random Room...";
    //        PhotonNetwork.JoinRandomRoom(); // 빈 방을 찾는데 실패하면 OnJoinRandomFailed() 함수 자동 호출
    //    }
    //    else
    //    {
    //        connectionInfoText.text = "Offline : Connection Disabled - Try reconnecting...";

    //        PhotonNetwork.ConnectUsingSettings();   // 접속에 실패해도 재접속 시도
    //    }
    //}

    //public override void OnJoinRandomFailed(short returnCode, string message)
    //{
    //    connectionInfoText.text = "There is no empty room, Creating new Room.";
    //    PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 8 });
    //}

    //public override void OnJoinedRoom()
    //{
    //    PhotonNetwork.LocalPlayer.NickName = NickNameInput.text;
    //    connectionInfoText.text = "Connected with Room.";
    //    PhotonNetwork.LoadLevel("GamePlay");    // SceneManager.LoadScene을 사용하면 각 유저가 개별의 씬을 로드하기때문에(동기화가 안됨) 사용하면안됨

    //}
}
