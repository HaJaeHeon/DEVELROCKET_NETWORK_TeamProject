using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using Unity.Android.Gradle.Manifest;

public class GameFlowController : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private TMP_InputField emailField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private string roomName = "IntegratedTest";
    [SerializeField] private int maxPlayer = 4;

    [SerializeField] private int playerScore;
    [SerializeField] private int playerSeconds;

    private FirebaseAuth auth;
    private GameResultService service;
    bool isSaving;
    bool isAuthBusy;
    bool wantJoin;


    private async void Start()
    {
        statusText.text = "Firebase Loading...";
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (status == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            statusText.text = "Firebase is ready";
            await Task.Delay(1000);
            var user = auth.CurrentUser;
            if (user != null)
                statusText.text = $"Log-In : {user.Email}";
            else
                statusText.text = $"Log-Out";
        }
        else
        {
            Debug.LogError($"Firebase is not ready : {status}");
            return; // 흠
        }
    }

    public async void OnSignUpButton()
    {
        if (!CanStartAuth())
            return;
        
        isAuthBusy = true; // 락 거는 개념
        try
        {
            if (await SignUpAsync())
                PhotonNetwork.ConnectUsingSettings();
        }
        finally
        {
            isAuthBusy = false;
        }
    }

    public async void OnSignInButton()
    {
        if (!CanStartAuth())
            return;

        isAuthBusy = true;
        try
        {
            
            if (await SignInAsync())
                PhotonNetwork.ConnectUsingSettings();
        }
        finally
        {
            isAuthBusy = false;
        }
    }

    public void OnEnterButton()
    {
        if (auth == null)
        {
            Debug.Log("Firebase is not ready");
            return;
        }
        
        if(auth.CurrentUser == null)
        {
            Debug.Log("Is not Log-in");
            return;
        }

        ConnectAndJoin();
    }

    public void OnSignOutButton()
    {
        if (auth == null)
        {
            Debug.Log("Firebase is not ready");
            return;
        }

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        loginPanel.SetActive(true);
        Debug.Log("Log-Out");
    }

    private async Task<bool> SignUpAsync()
    {
        try
        {
            var result = await auth.CreateUserWithEmailAndPasswordAsync(emailField.text, passwordField.text);
            Debug.Log($"Sign-Up : {auth.CurrentUser.UserId}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Sigh-Up Failed :" + ex);
            return false;
        }
    }

    private async Task<bool> SignInAsync()
    {
        try
        {
            var result = await auth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text);
            Debug.Log($"Sign-In : {auth.CurrentUser.UserId}");
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Sigh-In Failed :" + ex);
            return false;
        }
    }

    private bool CanStartAuth()
    {
        if (auth == null || isAuthBusy)
        {
            Debug.LogWarning("Firebase is busy");
            return false;
        }
            
        if(string.IsNullOrEmpty(emailField.text) || string.IsNullOrEmpty(passwordField.text))
        {
            Debug.LogWarning("Please enter email and password.");
            return false;
        }

        return true;
    }


    private void ConnectAndJoin()
    {
        var user = auth?.CurrentUser;
        if (user == null)
            return;

        PhotonNetwork.NickName = user.Email.Split('@')[0];


        // 아직 연결 X => 마스터 서버 연결 시도
        // 연결만 => 방 입장 시도
        // 방에 있는 상황 => 방에 있음을 알려준다.
        if (PhotonNetwork.InRoom)
        {
            Debug.Log($"In Room : {PhotonNetwork.CurrentRoom.Name}");
            return;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            JoinRoom();
        }
        else if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("Enter Room");
        }
            

    }

    private void JoinRoom()
    {
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = maxPlayer }, TypedLobby.Default);
        Debug.Log("Enter Room");
    }


    public override void OnConnectedToMaster()
    {
        Debug.Log("Master Server Connect");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Master Server Disconnect : {cause}");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Enter Room : {PhotonNetwork.CurrentRoom.Name}");
        loginPanel.SetActive(false);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Enter Room Fail : {message}");
    }

    [PunRPC]
    private async void RpcGameEnded(int finalScore, int playSeconds)
    {
        Debug.Log($"Game End Score : {finalScore} | playTime : {playSeconds}");
        if (PhotonNetwork.IsMasterClient)
        {
            await SaveResultFlow(finalScore, playSeconds);
        }
    }


    private async Task SaveResultFlow(int finalScore, int playSeconds)
    {
        if(auth == null)
        {
            Debug.LogWarning("Firebase is not ready");
            return;
        }

        service = new GameResultService();

        bool ok = await service.SaveResultAsync(roomName, finalScore, playSeconds);

        //재시도 함수
        bool ok2 = await service.SaveWithRetryAsync(roomName, finalScore, playSeconds);
        if (ok)
        {
            Debug.Log("Save!");
        }
        else
        {
            Debug.Log("Save Failed");
        }
    }


    [ContextMenu("Game End")]
    public void EndGame(int finalScore, int playSeconds)
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("Is Not Room");
            return;
        }

        photonView.RPC(nameof(RpcGameEnded), RpcTarget.All, playerScore, playerSeconds);
    }

}
