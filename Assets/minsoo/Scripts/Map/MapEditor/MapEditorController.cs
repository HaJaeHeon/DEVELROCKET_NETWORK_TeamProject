using UnityEngine;

public class MapEditorController : MonoBehaviour
{
    [SerializeField] private MapEditorUI editorUI;
    [SerializeField] private TilePainter painter;

    private UserMapRepository repo;
    private UserMapRepository Repo => repo ??= new UserMapRepository();
    private UserMapData current;   // 편집 중인 맵 (한 번 저장하거나 불러오면 Id가 생김)
    private bool isBusy;

    private void OnEnable()
    {
        editorUI.CheckRequested += OnCheckRequestHandler;
        editorUI.SaveRequested += OnSaveRequestHandler;
        editorUI.PublishRequested += OnPublishRequestHandler;
        editorUI.LoadRequested += OnLoadRequestHandler;
        editorUI.SizeChanged += OnSizeChangeHandler;
    }

    private void OnDisable()
    {
        editorUI.CheckRequested -= OnCheckRequestHandler;
        editorUI.SaveRequested -= OnSaveRequestHandler;
        editorUI.PublishRequested -= OnPublishRequestHandler;
        editorUI.LoadRequested -= OnLoadRequestHandler;
        editorUI.SizeChanged -= OnSizeChangeHandler;
    }

    private void OnCheckRequestHandler()
    {
        var errors = MapValidator.Validate(painter.Grid);
        if (errors.Count == 0) editorUI.ShowMessage("OK! Map is valid.");
        else editorUI.ShowMessage(string.Join("\n", errors), true);
    }

    private async void OnSaveRequestHandler()
    {
        if (isBusy) return;   // 연속 클릭 방지

        string name = editorUI.MapName;
        if (string.IsNullOrEmpty(name))
        {
            editorUI.ShowMessage("Enter a map name.", true);
            return;
        }

        var data = UserMapData.Create(name, painter.Grid);
        if (current != null)
        {
            data.Id = current.Id;                // 같은 맵에 덮어쓰기
            data.Published = current.Published;  // 게시 상태 유지
        }

        isBusy = true;
        editorUI.ShowMessage("Saving...");
        try
        {
            await Repo.SaveAsync(data);
            current = data;
            editorUI.ShowMessage("Saved!");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            editorUI.ShowMessage("Save failed.", true);
        }
        finally
        {
            isBusy = false;
        }
    }

    private async void OnPublishRequestHandler()
    {
        if (isBusy) return;

        var errors = MapValidator.Validate(painter.Grid);
        if (errors.Count > 0)
        {
            editorUI.ShowMessage(string.Join("\n", errors), true);
            return;
        }
        if (string.IsNullOrEmpty(editorUI.MapName))
        {
            editorUI.ShowMessage("Enter a map name.", true);
            return;
        }

        var data = UserMapData.Create(editorUI.MapName, painter.Grid);
        if (current != null) data.Id = current.Id;
        data.Published = true;

        isBusy = true;
        editorUI.ShowMessage("Publishing...");
        try
        {
            await Repo.SaveAsync(data);      // 최신 내용 먼저 저장 (처음이면 여기서 Id 생성)
            await Repo.PublishAsync(data);   // 공개 목록에 등록
            current = data;
            editorUI.ShowMessage("Published!");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            editorUI.ShowMessage("Publish failed.", true);
        }
        finally
        {
            isBusy = false;
        }
    }

    private async void OnLoadRequestHandler()
    {
        if (isBusy) return;
        isBusy = true;
        editorUI.ShowMessage("Loading...");
        try
        {
            var maps = await Repo.ListMineAsync();
            if (maps.Count == 0)
            {
                editorUI.ShowMessage("No saved maps.", true);
                return;   // finally에서 isBusy는 풀림
            }

            var map = maps[0];               // 일단 가장 최근 맵 (나중에 목록 패널로 교체)
            painter.LoadGrid(map.ToGrid());
            editorUI.MapName = map.Name;
            current = map;                   // 이후 Save는 이 맵에 덮어씀
            editorUI.ShowMessage($"Loaded: {map.Name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
            editorUI.ShowMessage("Load failed.", true);
        }
        finally
        {
            isBusy = false;
        }
    }

    private void OnSizeChangeHandler(int x, int y)
    {
        painter.SizeChanged(x, y);
    }
}
