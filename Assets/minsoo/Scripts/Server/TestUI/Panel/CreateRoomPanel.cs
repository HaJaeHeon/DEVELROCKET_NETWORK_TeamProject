using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateRoomPanel : UIPanel
{
    
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField roomNameField;

    NetworkManager NM => NetworkManager.Instance;
    private void Awake()
    {
        createRoomButton.onClick.AddListener(() => NM.OnCreateRoom(roomNameField.text));
    }
}
