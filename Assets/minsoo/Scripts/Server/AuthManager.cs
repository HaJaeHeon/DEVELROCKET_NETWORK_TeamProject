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
    public bool IsBusy { get; private set; }

    public event Action OnInitialized;
    public event Action<bool> OnBusyChanged;
    public event Action<FirebaseUser> OnSignedIn;

    public event Action<string> OnAuthFailed;
    public event Action<string> OnNotice;

    //이메일 가입 인증 확인
    [SerializeField] bool requireEmailVerification = true;
    [SerializeField] bool autoLoginToggle = false;

    public ProfileRepository PR {get; private set; }

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

        if (status == DependencyStatus.Available)
        {
            Auth = FirebaseAuth.DefaultInstance;
            PR = new ProfileRepository();

            IsReady = true;
            OnInitialized?.Invoke();

            if (Auth.CurrentUser != null)
            {
                await TryAutoLoginAsync();
                Auth.SignOut(); // 테스트용 초기화
            }
                

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
        if (!autoLoginToggle)
            return;

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

        if (requireEmailVerification && !user.IsEmailVerified)
        {
            Auth.SignOut();
            return;
        }
        CompleteSignIn(user);
    }

    // 익명 로그인
    public Task AnonymousLogin() => RunAuthAsync(async () =>
    {
        var result = await Auth.SignInAnonymouslyAsync();
        FirebaseUser user = result.User;

        int rand = UnityEngine.Random.Range(0, 100);
        string guestName = "Player" + rand;
        while (!await PR.NicknameExistAsync(guestName))
        {
            rand = UnityEngine.Random.Range(0, 100);
            guestName = "Player" + rand;
        }

        await PR.CreateGeustProfileAsync(guestName);
        CompleteSignIn(user);
    });

    // 회원가입
    public Task SignUpAsync(string email, string password, string nickname) => RunAuthAsync(async () =>
    {

        if (await PR.NicknameExistAsync(nickname))
        {
            OnAuthFailed?.Invoke("The nickname already exists.");
            return;
        }

        await Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        FirebaseUser user = Auth.CurrentUser;

        if (requireEmailVerification)
        {
            await user.SendEmailVerificationAsync();
            OnNotice?.Invoke("Send verification email");
        }

        if(!await PR.CreateProfileAsync(nickname))
        {
            OnAuthFailed?.Invoke("DB creation failed");
        }

        Auth.SignOut();
    });


    // 로그인
    public Task SignInAsync(string email, string password) => RunAuthAsync(async () =>
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
            OnAuthFailed?.Invoke("Connecting. Please try again in a moment.");
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
        Debug.Log("UID : " + SignedInUser.UserId);
        OnSignedIn?.Invoke(user);
    }

    private static string ToMessage(Exception e)
    {
        if (e is NotSupportedException || e is NotImplementedException) return e.Message;

        Debug.Log($"{e.Message}");
        AuthError? code = GetAuthError(e);
        switch (code)
        {
            case AuthError.MissingEmail: return "Please enter your email address.";
            case AuthError.Failure: return "Failed";
            case AuthError.InvalidEmail: return "Empty email";
            case AuthError.OperationNotAllowed: return "Console Login Off";
            default: return $"Login failed.. ({code})";
        }
    }

}
