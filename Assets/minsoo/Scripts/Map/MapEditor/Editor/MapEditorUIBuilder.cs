using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// 열려 있는 씬의 Canvas에 맵 에디터 UI를 만들고 MapEditorUI 참조를 연결
// 디자인은 StartScene MainCanvas(LobbyPanel, RoomPanel)와 같은 스프라이트·폰트·크기 규칙을 따름
public static class MapEditorUIBuilder
{
    private const string RootName = "MapEditorUI";
    private const string SpriteRoot = "Assets/Imports/Layer Lab/GUI Pro-FantasyRPG/ResourcesData/Sprites/Component/";
    private const string FontPath = "Assets/Imports/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string TileSetPath = "Assets/minsoo/Scripts/Map/MapEditor/MapTileSet.asset";

    private static readonly string[] BrushNames = { "Block", "Road", "Build", "Start", "Goal" }; // TileType 순서
    private static readonly Color TextDark = new(0.19607843f, 0.19607843f, 0.19607843f, 1f);

    private static Sprite frameYellow, framePanel, btnGray, btnYellow, btnRed, btnBlue, btnCircle, iconHome, inputBg;
    private static TMP_FontAsset font;

    [MenuItem("Tools/Map Editor/Build UI")]
    private static void Build()
    {
        if (!LoadAssets()) return;

        Canvas canvas = FindRootCanvas() ?? CreateCanvas();
        SetupScaler(canvas);
        EnsureEventSystem();

        Transform old = canvas.transform.Find(RootName);
        if (old != null)
        {
            if (!EditorUtility.DisplayDialog("Map Editor UI", "기존 MapEditorUI를 지우고 다시 만들까요?", "다시 만들기", "취소"))
                return;
            Undo.DestroyObjectImmediate(old.gameObject);
        }

        RectTransform root = NewUI(RootName, canvas.transform);
        Stretch(root);
        var ui = root.gameObject.AddComponent<MapEditorUI>();

        // 왼쪽 위 제목 태그 (LobbyPanel TitleBackground_Image)
        RectTransform title = NewUI("TitleTag_Image", root);
        Place(title, new Vector2(0, 1), new Vector2(20, -20), new Vector2(440, 120));
        AddImage(title, btnGray, true);
        var titleText = AddText(title, "Title_Text (TMP)", "Map Editor", 72, Color.black, FontStyles.Bold, TextAlignmentOptions.Center);
        Inset(titleText.rectTransform, 10);

        // 오른쪽 위 홈 버튼 (LobbyPanel Home_Button)
        RectTransform home = NewUI("Home_Button", root);
        Place(home, new Vector2(1, 1), new Vector2(-20, -20), new Vector2(100, 100));
        Button homeButton = home.gameObject.AddComponent<Button>();
        homeButton.targetGraphic = AddImage(home, btnCircle, false);
        RectTransform homeIcon = NewUI("Home_Image", home);
        Place(homeIcon, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60, 60));
        AddImage(homeIcon, iconHome, false).raycastTarget = false;

        // 왼쪽 타일 팔레트 (LobbyPanel PlayerListBack_Image 스타일)
        RectTransform palette = SidePanel("Palette_Panel", root, new Vector2(0, 0.5f), new Vector2(20, 40), new Vector2(320, 700), "Tiles");
        var brushButtons = new Button[BrushNames.Length];
        var brushSwatches = new Image[BrushNames.Length];
        for (int i = 0; i < BrushNames.Length; i++)
        {
            RectTransform b = NewUI($"{BrushNames[i]}_Button", palette);
            Place(b, new Vector2(0.5f, 1), new Vector2(0, -110 - i * 112), new Vector2(260, 90));
            brushButtons[i] = b.gameObject.AddComponent<Button>();
            brushButtons[i].targetGraphic = AddImage(b, btnGray, true, 1.2f);

            RectTransform swatch = NewUI("Swatch_Image", b);
            Place(swatch, new Vector2(0, 0.5f), new Vector2(24, 0), new Vector2(56, 56));
            brushSwatches[i] = AddImage(swatch, null, false);
            brushSwatches[i].preserveAspect = true;
            brushSwatches[i].raycastTarget = false;

            var label = AddText(b, "Text (TMP)", BrushNames[i], 36, TextDark, FontStyles.Bold, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.offsetMin = new Vector2(96, 0);
        }

        // 오른쪽 맵 정보 패널 (RoomPanel RoomInfo_Back_Image 스타일)
        RectTransform info = SidePanel("Info_Panel", root, new Vector2(1, 0.5f), new Vector2(-20, 40), new Vector2(440, 560), "Map Info");
        var infoText = AddText(info, "Info_Text (TMP)", "", 34, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        SetTopBand(infoText.rectTransform, 110, 250, 40);
        var messageText = AddText(info, "Message_Text (TMP)", "", 28, Color.white, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        SetTopBand(messageText.rectTransform, 380, 150, 40);
        messageText.textWrappingMode = TextWrappingModes.Normal;

        // 하단 바 (LobbyPanel RoomBack_Image 프레임)
        RectTransform bar = NewUI("BottomBar_Image", root);
        bar.anchorMin = new Vector2(0, 0);
        bar.anchorMax = new Vector2(1, 0);
        bar.pivot = new Vector2(0.5f, 0);
        bar.offsetMin = new Vector2(20, 20);
        bar.offsetMax = new Vector2(-20, 190);
        AddImage(bar, frameYellow, true, 0.5f);

        RectTransform nameLabel = NewUI("MapName_Text (TMP)", bar);
        Place(nameLabel, new Vector2(0, 0.5f), new Vector2(40, 0), new Vector2(220, 80));
        AddText(nameLabel, "Text", "Map Name", 40, Color.black, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);

        TMP_InputField nameInput = CreateInputField(bar, new Vector2(270, 0), new Vector2(500, 80));

        Button checkButton = BarButton(bar, "Check_Button", "Check", btnBlue, 1.5f, 0);
        Button saveButton = BarButton(bar, "Save_Button", "Save", btnYellow, 1.2f, 1);
        Button publishButton = BarButton(bar, "Publish_Button", "Publish", btnRed, 1.2f, 2);
        Button loadButton = BarButton(bar, "Load_Button", "Load", btnGray, 1.2f, 3);
        Button clearButton = BarButton(bar, "Clear_Button", "Clear", btnGray, 1.2f, 4);

        // MapEditorUI 참조 연결
        var painter = Object.FindAnyObjectByType<TilePainter>();
        if (painter == null) Debug.LogWarning("[MapEditorUIBuilder] 씬에 TilePainter가 없습니다. MapEditorUI의 painter를 직접 연결하세요.");

        var so = new SerializedObject(ui);
        so.FindProperty("painter").objectReferenceValue = painter;
        so.FindProperty("tileSet").objectReferenceValue = AssetDatabase.LoadAssetAtPath<MapTileSet>(TileSetPath);
        SetArray(so.FindProperty("brushButtons"), brushButtons);
        SetArray(so.FindProperty("brushSwatches"), brushSwatches);
        so.FindProperty("normalSprite").objectReferenceValue = btnGray;
        so.FindProperty("selectedSprite").objectReferenceValue = btnYellow;
        so.FindProperty("infoText").objectReferenceValue = infoText;
        so.FindProperty("messageText").objectReferenceValue = messageText;
        so.FindProperty("mapNameInput").objectReferenceValue = nameInput;
        so.FindProperty("checkButton").objectReferenceValue = checkButton;
        so.FindProperty("saveButton").objectReferenceValue = saveButton;
        so.FindProperty("publishButton").objectReferenceValue = publishButton;
        so.FindProperty("loadButton").objectReferenceValue = loadButton;
        so.FindProperty("clearButton").objectReferenceValue = clearButton;
        so.FindProperty("homeButton").objectReferenceValue = homeButton;
        so.ApplyModifiedPropertiesWithoutUndo();

        Undo.RegisterCreatedObjectUndo(root.gameObject, "Build Map Editor UI");
        EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        Selection.activeGameObject = root.gameObject;
        Debug.Log("[MapEditorUIBuilder] MapEditorUI를 만들었습니다. 씬을 저장하세요 (Ctrl+S).");
    }

    // ---------- 에셋 ----------

    private static bool LoadAssets()
    {
        frameYellow = LoadSprite("Frame/StageFrame_04_Single_Yellow.png");
        framePanel = LoadSprite("Frame/StageFrame_02_Demo_Focus.png");
        btnGray = LoadSprite("Button/Button_Rectangle_01_Convex_Gray.Png");
        btnYellow = LoadSprite("Button/Button_Rectangle_01_Convex_Yellow.Png");
        btnRed = LoadSprite("Button/Button_Rectangle_01_Convex_Red.Png");
        btnBlue = LoadSprite("Button/Button_Rectangle_02_Blue.png");
        btnCircle = LoadSprite("Button/Button_Circle_01_Gray.Png");
        iconHome = LoadSprite("IconMisc/Icon_Home.png");
        inputBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");

        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null) Debug.LogError($"[MapEditorUIBuilder] 폰트를 찾을 수 없습니다: {FontPath}");

        return frameYellow && framePanel && btnGray && btnYellow && btnRed && btnBlue && btnCircle && iconHome && font;
    }

    private static Sprite LoadSprite(string relativePath)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpriteRoot + relativePath);
        if (sprite == null) Debug.LogError($"[MapEditorUIBuilder] 스프라이트를 찾을 수 없습니다: {SpriteRoot + relativePath}");
        return sprite;
    }

    // ---------- Canvas / EventSystem ----------

    private static Canvas FindRootCanvas()
    {
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.isRootCanvas) return c;
        return null;
    }

    private static Canvas CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.layer = LayerMask.NameToLayer("UI");
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
        return canvas;
    }

    // MainCanvas와 같은 기준 해상도
    private static void SetupScaler(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);
        Undo.RecordObject(scaler, "Setup Canvas Scaler");
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
    }

    // ---------- UI 조각 ----------

    // 옆 패널 + 위쪽 헤더 띠
    private static RectTransform SidePanel(string name, RectTransform parent, Vector2 anchor, Vector2 pos, Vector2 size, string header)
    {
        RectTransform panel = NewUI(name, parent);
        Place(panel, anchor, pos, size);
        AddImage(panel, framePanel, true);

        RectTransform strip = NewUI("Header_Image", panel);
        Place(strip, new Vector2(0.5f, 1), Vector2.zero, new Vector2(size.x, 80));
        AddImage(strip, framePanel, true).raycastTarget = false;
        AddText(strip, "Header_Text (TMP)", header, 36, Color.white, FontStyles.Bold, TextAlignmentOptions.Center);
        return panel;
    }

    // 하단 바 오른쪽 버튼 줄 (190×90, 간격 205)
    private static Button BarButton(RectTransform bar, string name, string label, Sprite sprite, float ppuMultiplier, int index)
    {
        RectTransform rt = NewUI(name, bar);
        Place(rt, new Vector2(0, 0.5f), new Vector2(810 + index * 205, 0), new Vector2(190, 90));
        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = AddImage(rt, sprite, true, ppuMultiplier);
        AddText(rt, "Text (TMP)", label, 40, TextDark, FontStyles.Bold, TextAlignmentOptions.Center);
        return button;
    }

    // CreateRoom_Panel RoomName_InputField 스타일
    private static TMP_InputField CreateInputField(RectTransform parent, Vector2 pos, Vector2 size)
    {
        var go = TMP_DefaultControls.CreateInputField(new TMP_DefaultControls.Resources { inputField = inputBg });
        go.name = "MapName_InputField (TMP)";
        go.layer = parent.gameObject.layer;
        foreach (Transform t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = go.layer;

        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        Place(rt, new Vector2(0, 0.5f), pos, size);

        var input = go.GetComponent<TMP_InputField>();
        input.characterLimit = 24;
        input.fontAsset = font;

        var text = (TextMeshProUGUI)input.textComponent;
        text.font = font;
        text.fontSize = 50;
        text.color = TextDark;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        var placeholder = (TextMeshProUGUI)input.placeholder;
        placeholder.font = font;
        placeholder.text = "Enter Map Name...";
        placeholder.fontSize = 40;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.color = new Color(TextDark.r, TextDark.g, TextDark.b, 0.5f);
        placeholder.alignment = TextAlignmentOptions.MidlineLeft;
        return input;
    }

    // ---------- 공통 헬퍼 ----------

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    // anchor와 pivot을 같은 점에 두고 위치·크기 지정
    private static void Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void Inset(RectTransform rt, float padding)
    {
        rt.offsetMin = new Vector2(padding, padding);
        rt.offsetMax = new Vector2(-padding, -padding);
    }

    // 부모 위쪽에서 top만큼 내려온 곳에 높이 height인 가로 띠
    private static void SetTopBand(RectTransform rt, float top, float height, float sidePadding)
    {
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.offsetMin = new Vector2(sidePadding, -top - height);
        rt.offsetMax = new Vector2(-sidePadding, -top);
    }

    private static Image AddImage(RectTransform rt, Sprite sprite, bool sliced, float ppuMultiplier = 1f)
    {
        var image = rt.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.pixelsPerUnitMultiplier = ppuMultiplier;
        return image;
    }

    // 부모를 꽉 채우는 TMP 텍스트
    private static TextMeshProUGUI AddText(RectTransform parent, string name, string text, float size, Color color, FontStyles style, TextAlignmentOptions align)
    {
        RectTransform rt = NewUI(name, parent);
        Stretch(rt);
        var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = align;
        tmp.textWrappingMode = TextWrappingModes.NoWrap;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void SetArray(SerializedProperty prop, Object[] values)
    {
        prop.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }
}
