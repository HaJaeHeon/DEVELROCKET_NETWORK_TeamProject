using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test_PlayerColor : MonoBehaviourPunCallbacks
{
    private const string ColorKey = "PlayerColor";
    [SerializeField] private TMP_Dropdown dropdown;

    private MyColor[] colors;
    private MyColor selectedColor;
    private bool initialized;

    private void Awake()
    {
        if (dropdown == null)
        {
            Debug.LogError("색상 선택 목록이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        colors = (MyColor[])Enum.GetValues(typeof(MyColor));
        if (colors.Length == 0)
        {
            enabled = false;
            return;
        }

        int selectedIndex = Mathf.Clamp(dropdown.value, 0, colors.Length - 1);
        var player = PhotonNetwork.LocalPlayer;
        if (player != null && player.CustomProperties.TryGetValue(ColorKey, out object value)
            && value is int savedColor)
        {
            int savedIndex = Array.IndexOf(colors, (MyColor)savedColor);
            if (savedIndex >= 0)
                selectedIndex = savedIndex;
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(
            new List<string>(Enum.GetNames(typeof(MyColor)))
        );

        dropdown.SetValueWithoutNotify(selectedIndex);
        selectedColor = colors[selectedIndex];
        initialized = true;
        dropdown.onValueChanged.AddListener(OnColorSelected);
    }

    private void Start()
    {
        PublishSelectedColor();
    }

    private void OnDestroy()
    {
        if (dropdown != null)
            dropdown.onValueChanged.RemoveListener(OnColorSelected);
    }

    private void OnColorSelected(int index)
    {
        if (!initialized || index < 0 || index >= colors.Length)
            return;

        selectedColor = colors[index];
        PublishSelectedColor();
    }

    public override void OnJoinedRoom()
    {
        // 방 입장 전에 선택한 색상도 입장 후 다시 반영합니다.
        PublishSelectedColor();
    }

    private void PublishSelectedColor()
    {
        if (!initialized || PhotonNetwork.LocalPlayer == null)
            return;

        // 방 밖에서는 로컬 속성에 저장하고, 방 안에서는 다른 참여자에게 전송합니다.
        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable
            {
                { ColorKey, (int)selectedColor }
            }
        );
    }
}
