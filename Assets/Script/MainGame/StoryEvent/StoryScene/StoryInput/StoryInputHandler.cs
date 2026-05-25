using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StoryInputHandler : MonoBehaviour
{
    public event System.Action OnAdvanceRequested;
    public event System.Action OnSkipStarted;
    public event System.Action OnSkipEnded;

    public bool IsSkipping { get; private set; }

    void Update()
    {
        var data = new StoryInputData
        {
            SkipHeld = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl),
            IsPointerOverUI = IsPointerOverInteractable(),
            AdvanceRequested = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)
        };

        // 分流邏輯集中在此
        if (data.AdvanceRequested && !data.IsPointerOverUI)
            OnAdvanceRequested?.Invoke();

        if (data.SkipHeld)
            OnSkipStarted?.Invoke();
        else
            OnSkipEnded?.Invoke();
    }

    private bool IsPointerOverInteractable()
    {
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // 只有掛了 Selectable（Button/Toggle/Slider）的物件才算擋住
            if (result.gameObject.GetComponentInParent<Selectable>() != null)
                return true;
        }

        return false;
    }
}
