using UnityEngine;

public class CursorRoot : MonoBehaviour
{
    public static RectTransform Root { get; private set; }

    private void Awake()
    {
        Root = GetComponent<RectTransform>();
    }

    private void OnDestroy()
    {
        if (Root == transform)
            Root = null;
    }
}
