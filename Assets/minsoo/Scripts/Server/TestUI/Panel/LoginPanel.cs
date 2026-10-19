using Firebase.Auth;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : UIPanel
{
    [SerializeField] private Button signInButton;
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button titleButton;

    [Header("로그인 UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("팝업 UI")]
    [SerializeField] private GameObject PopupPanel;
    [SerializeField] private TMP_Text messageText;

    bool proceeding;
    AuthManager AM => AuthManager.Instance;
    Color errorColor = new Color(0.9f, 0.3f, 0.3f);
    WaitForSeconds wait = new WaitForSeconds(1.0f);

    private void Awake()
    {
        var ui = GetComponentInParent<PanelController>(true);

        signInButton.onClick.AddListener(() => AM.SignInAsync(emailInput.text, passwordInput.text));
        signUpButton.onClick.AddListener(() => ui.Open(PanelId.Login, PanelId.SignUp));
        titleButton.onClick.AddListener(() => ui.Open(PanelId.Title));
    }

    private void OnEnable()
    {
        AM.OnInitialized += RefreshHandler;
        AM.OnBusyChanged += RefreshHandler;

        // 팝업UI
        AM.OnAuthFailed += ShowError;
        AM.OnNotice += ShowNotice;
    }

    private void OnDisable()
    {
        AM.OnInitialized -= RefreshHandler;
        AM.OnBusyChanged -= RefreshHandler;

        AM.OnAuthFailed -= ShowError;
        AM.OnNotice -= ShowNotice;
    }


    private void RefreshHandler() => RefreshInteractable();
    private void RefreshHandler(bool busy) => RefreshInteractable();

    private void RefreshInteractable()
    {
        bool canInput = AM.IsReady && !AM.IsBusy && !proceeding;

        //signInButton.interactable = canInput;
        //signUpButton.interactable = canInput;
        //signOutButton.interactable = canInput;
        //loadingPanel.SetActive(!canInput);
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
