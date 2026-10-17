using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class ButtonTowerPool : SingletonBehavior<ButtonTowerPool>
{
    public enum UIType
    {
        Shop,
        Inventory,
    }
    [Header("프리팹")]
    [SerializeField] private GameObject buttonTowerPrefab;

    [Header("패널")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject shopContent;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject inventoryContent;


    private const int SHOPMAXCOUNT = 3;

    private IObjectPool<GameObject> pool;


    public override void Awake()
    {
        base.Awake();
        pool = new ObjectPool<GameObject>
        (
            createFunc: () => Instantiate(buttonTowerPrefab),
            actionOnGet: obj => obj.SetActive(true),
            actionOnRelease: obj => obj.SetActive(false),
            actionOnDestroy: obj => Destroy(obj),
            collectionCheck: true,
            defaultCapacity: 10,
            maxSize: 100
            );
    }

    private void Start()
    {
        for (int i = 0; i < SHOPMAXCOUNT; i++)
        {
            ButtonTowerPool.Instance.Spawn(UIType.Shop);
        }
    }


    public GameObject Spawn(UIType type)
    {
        GameObject buttonTower = pool.Get();

        InventoryDraggable drag = buttonTower.transform.GetChild(0).GetComponent<InventoryDraggable>();
        PresetSODataContainer so = buttonTower.transform.GetChild(0).GetComponent<PresetSODataContainer>();
        Image towerImage = buttonTower.transform.GetChild(0).GetComponent<Image>();
        Button towerButton = buttonTower.GetComponent<Button>();
        drag.canvas = shopPanel.transform.root.GetComponent<Canvas>();

        if (type == UIType.Shop)
        {
            buttonTower.transform.SetParent(shopContent.transform, false);
            
            drag.enabled = false;

            towerImage.raycastTarget = false;
            towerImage.sprite = so.data.GetIcon();

            towerButton.enabled = true;
            BuyTower bt = shopPanel.GetComponent<BuyTower>();
            towerButton.onClick.AddListener(() => bt.GetTowerData(buttonTower));
        }
        else if (type == UIType.Inventory)
        {
            buttonTower.transform.SetParent(inventoryContent.transform, false);

            drag.enabled = true;
            drag.imageId = $"{so.data.Id}";
            drag.rootPanel = shopPanel;

            towerImage.raycastTarget = true;

            towerButton.enabled = false;
        }

        return buttonTower;
    }

    public void Return(GameObject obj, UIType type)
    {
        //if(type == UIType.Shop)
        //{
        //    obj.GetComponent<Button>().onClick.RemoveAllListeners();
        //}
        obj.GetComponent<Button>().onClick.RemoveAllListeners();

        pool.Release(obj);
    }

    private void OnDestroy()
    {
        pool?.Clear();
    }
}
