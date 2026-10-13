using Firebase.Auth;
using Photon.Pun;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestUI : MonoBehaviour
{
    [Header("로그인 UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signOutButton;
    

    [Header("회원가입 UI")]
    [SerializeField] private GameObject SignUpPanel;
    [SerializeField] private TMP_InputField createEmailInput;
    [SerializeField] private TMP_InputField createPasswordInput;
    [SerializeField] private TMP_InputField createNickname;
    [SerializeField] private Button signUpButton;

    [Header("팝업 UI")]
    [SerializeField] private GameObject PopupPanel;
    [SerializeField] private TMP_Text messageText;

    bool proceeding;
    AuthManager AM => AuthManager.Instance;
    private ProfileRepository pr;
    Color errorColor = new Color(0.9f, 0.3f, 0.3f);
    WaitForSeconds wait = new WaitForSeconds(3.0f);

    private void Awake()
    {
        signUpButton.onClick.AddListener(() => AM.SignUpAsync(emailInput.text, passwordInput.text, createNickname.text));
        signInButton.onClick.AddListener(() => AM.SignInAsync(emailInput.text, passwordInput.text));
        signOutButton.onClick.AddListener(() => AM.SignOut());
    }

    private void OnEnable()
    {
        AM.OnInitialized += RefreshHandler;
        AM.OnBusyChanged += RefreshHandler;
        AM.OnSignedIn += SignInHandlerAsync;

        // 팝업UI
        AM.OnAuthFailed += ShowError;
        AM.OnNotice += ShowNotice;
    }

    private void OnDisable()
    {
        AM.OnInitialized -= RefreshHandler;
        AM.OnBusyChanged -= RefreshHandler;
        AM.OnSignedIn -= SignInHandlerAsync;

        AM.OnAuthFailed -= ShowError;
        AM.OnNotice -= ShowNotice;
    }

    private void RefreshHandler() => RefreshInteractable();
    private void RefreshHandler(bool busy) => RefreshInteractable();

    private void RefreshInteractable()
    {
        bool canInput = AM.IsReady && !AM.IsBusy && !proceeding;

        signInButton.interactable = canInput;
        signUpButton.interactable = canInput;
        signOutButton.interactable = canInput;
        //loadingPanel.SetActive(!canInput);
    }

    private async void SignInHandlerAsync(FirebaseUser user)
    {
        if (proceeding) return;
        proceeding = true;

        try
        {
            if (this == null)
                return;
            // 로비로 이동
            string notice = await NetworkManager.Instance.Connect();
            if (string.IsNullOrEmpty(notice))
                ShowNotice("Connect!");
            else
                ShowError(notice);
        }
        catch(Exception e)
        {
            if (this == null)
                return;
            proceeding = false;
            RefreshInteractable();
        }

    }

    private void ShowError(string message)
    {
        PopupPanel.SetActive(true);
        messageText.color = errorColor;
        messageText.text = message;

        StartCoroutine("ClosePopup");
    }

    private void ShowNotice(string message)
    {
        PopupPanel.SetActive(true);
        messageText.color = Color.black;
        messageText.text = message;

        StartCoroutine("ClosePopup");
    }

    private IEnumerator ClosePopup()
    {
        yield return wait;
        PopupPanel.SetActive(false);
    }

}
