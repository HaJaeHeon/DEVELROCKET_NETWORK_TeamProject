using ExitGames.Client.Photon;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;


public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser SignedInUser { get; private set; }

    public bool IsReady { get; private set; }
    public bool IsBusy {  get; private set; }

    public event Action OnInitialized;
    public event Action<bool> OnBusyChanged;
    public event Action<FirebaseUser> OnSignedIn;

    public event Action<string> OnAuthFailed;
    public event Action<string> OnNotice;

    //이메일 가입 인증 확인
    [SerializeField] bool requireEmailVerification = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }

    private async void Start()
    {
        if (IsReady) 
            return;

        var status = await FirebaseApp.CheckAndFixDependenciesAsync();

        if(status == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;

            IsReady = true;
            OnInitialized?.Invoke();

            if (Auth.CurrentUser != null)
                await TryAutoLoginAsync();

        }
        else
        {
            Debug.Log($"Firebase 초기화 실패:{status}");
            return;
        }
    }

    // 자동 로그인
    private async Task TryAutoLoginAsync()
    {
        SetBusy(true);
        try
        {
            // 유저 로그인 새로고침(계정 삭제/정지 시 catch로 잡힌다)
            await Auth.CurrentUser.ReloadAsync();
        }
        catch (Exception e)
        {
            // AuthErro가 계정 문제라면 자동 로그인 취소
            AuthError? code = GetAuthError(e);
            if (code == AuthError.UserNotFound || code == AuthError.UserDisabled ||
                code == AuthError.InvalidUserToken || code == AuthError.UserTokenExpired)
            {
                Auth.SignOut();
                SetBusy(false);
                return;
            }
        }
        SetBusy(false);


        FirebaseUser user = Auth.CurrentUser;
        if (user == null) return;

        if(requireEmailVerification && !user.IsEmailVerified)
        {
            Auth.SignOut();
            return;
        }
        CompleteSignIn(user);
    }


    // 회원가입
    public Task SignUp(string email, string password, string nickname) => RunAuthAsync(async () =>
    {
        await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        FirebaseUser user = Auth.CurrentUser;

        if (requireEmailVerification)
        {
            await user.SendEmailVerificationAsync();
            OnNotice?.Invoke("인증 메일 전송!");
        }
       
        Auth.SignOut();
    });


    // 로그인
    public Task SignIn(string email, string password) => RunAuthAsync(async () =>
    {
        await Auth.SignInWithEmailAndPasswordAsync(email, password);
        FirebaseUser user = Auth.CurrentUser;

        // 인증 메일 재전송
        if (requireEmailVerification && !user.IsEmailVerified)
        {
            await user.SendEmailVerificationAsync();
            Auth.SignOut();
            OnAuthFailed?.Invoke("이메일 인증이 되어있지 않습니다. 인증 메일을 재전송합니다.");
            return;
        }
            CompleteSignIn(user);

    });

    // 로그아웃
    public void SignOut()
    {
        if (Auth == null)
            return;

        Auth.SignOut();
        SignedInUser = null;
    }

    private async Task RunAuthAsync(Func<Task> action)
    {
        if (!IsReady)
        {
            OnAuthFailed?.Invoke("초기화 중입니다. 잠시 후 다시 시도해 주세요.");
            return;
        }
        if (IsBusy) return;

        SetBusy(true);
        try
        {
            await action();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AuthManager] {e}");
            OnAuthFailed?.Invoke(ToMessage(e));
        }
        finally
        {
            SetBusy(false);
        }
    }

    private static AuthError? GetAuthError(Exception e)
    {
        while (e != null)
        {
            if (e is FirebaseException fe) return (AuthError)fe.ErrorCode;
            e = e is AggregateException ae && ae.InnerExceptions.Count > 0
                ? ae.InnerExceptions[0]
                : e.InnerException;
        }
        return null;
    }

    private void SetBusy(bool busy)
    {
        if (IsBusy == busy) return;
        IsBusy = busy;
        OnBusyChanged?.Invoke(busy);
    }

    private void CompleteSignIn(FirebaseUser user)
    {
        SignedInUser = user;
        OnSignedIn?.Invoke(user);
    }

    private static string ToMessage(Exception e)
    {
        if (e is NotSupportedException || e is NotImplementedException) return e.Message;

        AuthError? code = GetAuthError(e);
        switch (code)
        {
            case AuthError.MissingEmail: return "이메일을 입력해주세요.";
            default: return $"로그인에 실패했습니다. ({code})";
        }
    }

}
