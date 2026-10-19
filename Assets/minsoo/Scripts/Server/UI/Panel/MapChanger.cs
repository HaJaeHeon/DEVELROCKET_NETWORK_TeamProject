using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapChanger : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown mapsDropdown;
    [SerializeField] private Image mapImage;
    [SerializeField] private List<Sprite> mapThumnail;

    [Header("MapDB")]
    [SerializeField] private MapDB mapDB;

    private void Awake()
    {
        List<string> mapName = new List<string>();
        
        foreach (var map in mapDB.MapDatas)
        {
            mapName.Add(map.Name);
            mapThumnail.Add(map.Thumnail);
        }

        mapsDropdown.ClearOptions();
        mapsDropdown.options.Add(new TMP_Dropdown.OptionData("Random"));
        mapsDropdown.AddOptions(mapName);

        mapsDropdown.onValueChanged.AddListener(OnSelectMap);
    }


    private void OnSelectMap(int index)
    {
        mapImage.sprite = mapThumnail[index];
        mapImage.SetNativeSize();
        Debug.Log(index);
    }

    private void OnDestroy()
    {
        mapsDropdown.onValueChanged.RemoveAllListeners();
    }
}
