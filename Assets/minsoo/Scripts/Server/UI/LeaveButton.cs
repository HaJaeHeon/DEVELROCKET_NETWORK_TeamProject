using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class LeaveButton : MonoBehaviour
{
    [SerializeField] private Button leaveButton;


    private void Awake()
    {
        leaveButton.onClick.AddListener(() => PhotonNetwork.LeaveRoom());
    }
}
