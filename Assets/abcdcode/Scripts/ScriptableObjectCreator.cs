using UnityEditor;
using UnityEngine;
using System;
using System.Linq;

#if UNITY_EDITOR
/// <summary>
/// ScriptableObject 광역적으로 뽑아쓰는 에디터 스크립트
/// 챗지피티 작품
/// </summary>
public class ScriptableObjectCreator : EditorWindow
{

    [MenuItem("Assets/Create/Game Asset...")]
    private static void Open()
    {
        GetWindow<ScriptableObjectCreator>("Create Asset");
    }
    private void OnEnable()
    {
        types = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(t =>
                t.IsSubclassOf(typeof(SOData)) &&
                !t.IsAbstract)
            .OrderBy(t => t.Name)
            .ToArray();
    }
    public Type[] types;


    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var type in types)
        {
            if (GUILayout.Button(type.Name))
            {
                CreateAsset(type);
            }
        }

        EditorGUILayout.EndScrollView();
    }
    private static void CreateAsset(Type type)
    {
        var asset = ScriptableObject.CreateInstance(type);
        
        if(asset is SOData so)
        {
            so.SODataInit();
        }
        
        ProjectWindowUtil.CreateAsset(
            asset,
            $"New {type.Name}.asset");
    }
    private Vector2 scroll;
}
#endif