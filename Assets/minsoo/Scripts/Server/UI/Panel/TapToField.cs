using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabToField : MonoBehaviour
{
    [SerializeField] private List<TMP_InputField> fields = new();

    private void Awake()
    {
        fields.Clear();
        fields.AddRange(GetComponentsInChildren<TMP_InputField>(true));
    }

    private void OnEnable()
    {
        // 패널이 켜질 때 첫 번째 필드에 포커스
        if (fields.Count > 0)
        {
            fields[0].Select();
            fields[0].ActivateInputField();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Tab)) return;
        if (fields.Count == 0) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;
        int index = fields.FindIndex(f => f.gameObject == current);

        if (index < 0) return; // 내 패널의 필드가 아니면 무시

        int next = (index + 1) % fields.Count;
        fields[next].Select();
        fields[next].ActivateInputField();
    }
}