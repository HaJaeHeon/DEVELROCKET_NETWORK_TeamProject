using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignUpPanel : UIPanel
{
    [SerializeField] private Button signUpButton;

    [Header("회원가입 UI")]
    [SerializeField] private TMP_InputField createEmailInput;
    [SerializeField] private TMP_InputField createPasswordInput;
    [SerializeField] private TMP_InputField createNickname;

    [Header("팝업 UI")]
    [SerializeField] private GameObject PopupPanel;
    [SerializeField] private TMP_Text messageText;

    bool proceeding;
    AuthManager AM => AuthManager.Instance;
    private ProfileRepository pr;
    Color errorColor = new Color(0.9f, 0.3f, 0.3f);
    WaitForSeconds wait = new WaitForSeconds(1.0f);

    private void Awake()
    {
        var ui = GetComponentInParent<PanelController>(true);
        // 회원가입이 제대로 되었을 때만 가야함
        signUpButton.onClick.AddListener(() => AM.SignUpAsync(createEmailInput.text, createPasswordInput.text, createNickname.text));
        
    }

    private void OnEnable()
    {
        AM.OnAuthFailed += ShowError;
        AM.OnNotice += ShowNotice;
    }

    private void OnDisable()
    {
        AM.OnAuthFailed -= ShowError;
        AM.OnNotice -= ShowNotice;
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
