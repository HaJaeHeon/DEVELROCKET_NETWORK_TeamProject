using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

// 맵 에디터 화면 UI. Tools > Map Editor > Build UI 로 생성하고 참조도 자동으로 연결됨
public class MapEditorUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TilePainter painter;
    [SerializeField] private MapTileSet tileSet;

    [Header("Palette")]
    [SerializeField] private Button[] brushButtons;     // TileType 순서 (Block, Road, Build, Start, Goal)
    [SerializeField] private Image[] brushSwatches;
    [SerializeField] private Sprite normalSprite;       // 선택 안 된 버튼
    [SerializeField] private Sprite selectedSprite;     // 선택된 버튼

    [Header("Info")]
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Color messageColor = Color.white;
    [SerializeField] private Color errorColor = new(1f, 0.4f, 0.35f);

    [Header("Bottom Bar")]
    [SerializeField] private TMP_InputField mapNameInput;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button publishButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button clearButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private string lobbySceneName = "StartScene";

    [Header("SettingPanel")]
    [SerializeField] private TMP_InputField sizeXInput;
    [SerializeField] private TMP_InputField sizeYInput;
    [SerializeField] private Button okButton;
    [SerializeField] private int minSize = 5;
    [SerializeField] private int maxSize = 40;   // 맵 내용이 방 프로퍼티로 전송되므로 크기 제한

    // 검사, 저장, 게시, 불러오기는 MapEditorController에서 구독해서 처리
    public event Action CheckRequested;
    public event Action SaveRequested;
    public event Action PublishRequested;
    public event Action LoadRequested;
    public event Action<int, int> SizeChanged;

    public string MapName
    {
        get => mapNameInput.text.Trim();
        set => mapNameInput.text = value;
    }

    private void Awake()
    {
        for (int i = 0; i < brushButtons.Length; i++)
        {
            int type = i;
            brushButtons[i].onClick.AddListener(() => SelectBrush(type));
        }

        checkButton.onClick.AddListener(() => CheckRequested?.Invoke());
        saveButton.onClick.AddListener(() => SaveRequested?.Invoke());
        publishButton.onClick.AddListener(() => PublishRequested?.Invoke());
        loadButton.onClick.AddListener(() => LoadRequested?.Invoke());
        clearButton.onClick.AddListener(() => painter.Clear());
        homeButton.onClick.AddListener(() => SceneManager.LoadScene(lobbySceneName));
        okButton.onClick.AddListener(OnSizeOk);
        ApplySwatches();
    }

    private void OnEnable()
    {
        if (painter != null) painter.Changed += RefreshInfo;
    }

    private void OnDisable()
    {
        if (painter != null) painter.Changed -= RefreshInfo;
    }

    private void Start()
    {
        SelectBrush((int)TileType.Road);
        RefreshInfo();
        ShowMessage("Left click : Paint\nRight click : Erase\nMap View : Tap");
    }

    private void OnSizeOk()
    {
        if (!int.TryParse(sizeXInput.text, out int x) || !int.TryParse(sizeYInput.text, out int y)
            || x < minSize || y < minSize || x > maxSize || y > maxSize)
        {
            ShowMessage($"Size must be {minSize} ~ {maxSize}.", true);
            return;
        }
        SizeChanged?.Invoke(x, y);
    }

    public void SelectBrush(int type)
    {
        painter.SetBrush(type);
        for (int i = 0; i < brushButtons.Length; i++)
            brushButtons[i].image.sprite = i == type ? selectedSprite : normalSprite;
    }

    public void ShowMessage(string text, bool isError = false)
    {
        messageText.text = text;
        messageText.color = isError ? errorColor : messageColor;
    }

    private void RefreshInfo()
    {
        var grid = painter.Grid;
        if (grid == null) return;

        var counts = new int[Enum.GetValues(typeof(TileType)).Length];
        foreach (var t in grid) counts[(int)t]++;

        infoText.text =
            $"Size<pos=55%>{grid.GetLength(0)} x {grid.GetLength(1)}\n" +
            $"Road<pos=55%>{counts[(int)TileType.Road]}\n" +
            $"Build<pos=55%>{counts[(int)TileType.Build]}\n" +
            $"Start<pos=55%>{counts[(int)TileType.Start]}\n" +
            $"Goal<pos=55%>{counts[(int)TileType.Goal]}";
    }

    // 팔레트 견본: 비주얼 타일이 비어 있으면 로직 타일 그림을 씀
    private void ApplySwatches()
    {
        if (tileSet == null) return;
        for (int i = 0; i < brushSwatches.Length; i++)
        {
            Sprite sprite = SpriteOf(tileSet.Visual((TileType)i));
            if (sprite == null) sprite = SpriteOf(tileSet.Logic((TileType)i));
            brushSwatches[i].sprite = sprite;
            brushSwatches[i].enabled = sprite != null;
        }
    }

    private static Sprite SpriteOf(TileBase tile) => tile switch
    {
        Tile t => t.sprite,
        RuleTile r => r.m_DefaultSprite,
        _ => null
    };

}
