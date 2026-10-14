using ExitGames.Client.Photon;
using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Test_PlayerColor : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private MyColor[] colors;

    private void Awake()
    {
        colors = (MyColor[])Enum.GetValues(typeof(MyColor));

        dropdown.ClearOptions();
        dropdown.AddOptions(
            new List<string>(Enum.GetNames(typeof(MyColor)))
        );

        dropdown.onValueChanged.AddListener(OnColorSelected);
    }

    private void Start()
    {
        OnColorSelected(dropdown.value);
    }

    private void OnDestroy()
    {
        dropdown.onValueChanged.RemoveListener(OnColorSelected);
    }

    private void OnColorSelected(int index)
    {
        MyColor selectedColor = colors[index];

        PhotonNetwork.LocalPlayer.SetCustomProperties(
            new Hashtable
            {
                { "PlayerColor", (int)selectedColor }
            }
        );
    }
}
