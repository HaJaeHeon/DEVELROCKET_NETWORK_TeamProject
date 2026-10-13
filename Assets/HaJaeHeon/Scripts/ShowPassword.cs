using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowPassword : MonoBehaviour
{
    [SerializeField] private Sprite OpenEyeSprite;
    [SerializeField] private Sprite closeEyeSprite;

    [SerializeField] private Button eyeButton;
    [SerializeField] private TMP_InputField passwordInputField;

    private Image buttonImage;

    private bool isShow;

    private void Awake()
    {
        buttonImage = eyeButton.GetComponent<Image>();
    }

    private void OnEnable()
    {
        eyeButton.onClick.AddListener(OnClickShowPassword);
    }

    private void OnDisable()
    {
        eyeButton.onClick.RemoveListener(OnClickShowPassword);
    }

    public void OnClickShowPassword()
    {
        if(!isShow)
        {
            isShow = !isShow;
            buttonImage.sprite = closeEyeSprite;
            passwordInputField.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            isShow = !isShow;
            buttonImage.sprite = OpenEyeSprite;
            passwordInputField.contentType = TMP_InputField.ContentType.Password;
        }

        passwordInputField.ForceLabelUpdate();
    }
}
