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

    private bool isShow = true;

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
        isShow = !isShow;
        if (!isShow)
        {
            buttonImage.sprite = OpenEyeSprite;
            passwordInputField.inputType = TMP_InputField.InputType.Standard;
        }
        else
        {
            buttonImage.sprite = closeEyeSprite;
            passwordInputField.inputType = TMP_InputField.InputType.Password;
        }

        passwordInputField.ForceLabelUpdate();
    }
}
