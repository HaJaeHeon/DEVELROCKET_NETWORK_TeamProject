using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : UIPanel
{
    
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField roomNameField;
    [SerializeField] private TMP_Dropdown playerCountDropdown;
    [SerializeField] private TMP_InputField roomPassword;
    [SerializeField] private Toggle OfflineToggle;
    [SerializeField] private Toggle PrivateToggle;

    
    NetworkManager NM => NetworkManager.Instance;
    private void Awake()
    {
        roomPassword.interactable = false;

        createRoomButton.onClick.AddListener(OnClickCreatRoomButton);
        OfflineToggle.onValueChanged.AddListener(isOn => OnOfflineMode(isOn));
        PrivateToggle.onValueChanged.AddListener(OnPrivateRoom);
    }

    private void OnClickCreatRoomButton()
    {
        string name = string.IsNullOrWhiteSpace(roomNameField.text) ? null : roomNameField.text;
        string pw = roomPassword.interactable && !string.IsNullOrEmpty(roomPassword.text) ? roomPassword.text : null;
        int playerCount = int.Parse(playerCountDropdown.options[playerCountDropdown.value].text);

        NM.OnCreateRoom(OfflineToggle.isOn, name, playerCount, pw);
    }

    private void OnOfflineMode(bool isOffline)
    {

        roomNameField.interactable = !isOffline;
        playerCountDropdown.interactable = !isOffline;
        playerCountDropdown.value = isOffline ? 3 : 0;
        PrivateToggle.interactable = !isOffline;

        if (!PrivateToggle.isOn)
            return;
        roomPassword.interactable = !isOffline;

    }

    private void OnPrivateRoom(bool isPrivate)
    {
        roomPassword.interactable = isPrivate;
    }
}
