using System.Collections.Generic;
using UnityEngine;

public class GlobalModifierView : MonoBehaviour
{
    [SerializeField] private Transform _container; // 具有 Horizontal Layout Group 的節點
    [SerializeField] private GlobalModifierItemView _itemPrefab; // 單個圖示的 Prefab

    private List<GlobalModifierItemView> _activeItems = new List<GlobalModifierItemView>();

    public void Refresh(List<ModifierUIData> modifiers)
    {
        // 1. 清理舊圖示
        foreach (var item in _activeItems) Destroy(item.gameObject);
        _activeItems.Clear();

        // 2. 生成新圖示
        foreach (var data in modifiers)
        {
            var newItem = Instantiate(_itemPrefab, _container);
            newItem.Setup(data);
            _activeItems.Add(newItem);
        }
    }
}
