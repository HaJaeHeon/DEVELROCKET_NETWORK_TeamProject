using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapChanger : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private Sprite randomThumbnail;

    [Header("MapDB")]
    [SerializeField] private MapDB mapDB;

    private void Awake()
    {
        List<string> mapName = new List<string>();
        
        foreach (var map in mapDB.MapDatas)
        {
            mapName.Add(map.Name);
        }

        mapsDropdown.ClearOptions();
        mapsDropdown.options.Add(new TMP_Dropdown.OptionData("Random"));
        mapsDropdown.AddOptions(mapName);

        mapsDropdown.onValueChanged.AddListener(OnSelectMap);
    }


    private void OnSelectMap(int index)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        string id = index == 0 ? RoomProps.RandomId : mapDB.MapDatas[index - 1].Id;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { RoomProps.MapIdKey, id } });

    }

    public override void OnRoomPropertiesUpdate(Hashtable changed)
    {
        if (changed.ContainsKey(RoomProps.MapIdKey)) ApplyFromRoom();
    }

    // 방장이 바뀌면 조작 권한 갱신
    public override void OnMasterClientSwitched(Player newMaster) => UpdateInteractable();

    public override void OnEnable()
    {
        base.OnEnable();          
        UpdateInteractable();
        ApplyFromRoom(); 
    }

    private void UpdateInteractable() => mapsDropdown.interactable = PhotonNetwork.IsMasterClient;

    private void ApplyFromRoom()
    {
        var room = PhotonNetwork.CurrentRoom;
        string id = room != null && room.CustomProperties.TryGetValue(RoomProps.MapIdKey, out var v) ? v as string : RoomProps.RandomId;

        int index = 0;   // Random
        for (int i = 0; i < mapDB.MapDatas.Count; i++)
            if (mapDB.MapDatas[i].Id == id) { index = i + 1; break; }

        mapsDropdown.SetValueWithoutNotify(index);
        mapImage.sprite = index == 0
            ? randomThumbnail                        
            : mapDB.MapDatas[index - 1].Thumbnail;    
        mapImage.SetNativeSize();
    }

    private void OnDestroy()
    {
        mapsDropdown.onValueChanged.RemoveAllListeners();
    }
}
