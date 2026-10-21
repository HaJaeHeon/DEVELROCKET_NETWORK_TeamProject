using UnityEngine;

public class MapEditorController : MonoBehaviour
{
    [SerializeField] private MapEditorUI editorUI;
    [SerializeField] private TilePainter painter;

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

    private void OnSaveRequestHandler()
    {

    }

    private void OnPublishRequestHandler()
    {
        var errors = MapValidator.Validate(painter.Grid);
        if(errors.Count == 0)
        {

        }
        else editorUI.ShowMessage(string.Join("\n", errors), true);


    }

    private void OnLoadRequestHandler()
    {

    }

    private void OnSizeChangeHandler(int x, int y)
    {
        painter.SizeChanged(x, y);
    }
}
