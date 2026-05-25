using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Condition/Micro/UIButtonClicked")]
public class UIButtonClickedCondition : MicroConditionSO
{
    [Header("目標按鈕在 Registry 中的 ID (例如: Target_EndPhaseBtn)")]
    public string TargetButtonID;

    // --- 內部狀態 (必須加上 [NonSerialized]，避免狀態殘留在 ScriptableObject 裡) ---
    [NonSerialized] private bool _isClicked;
    [NonSerialized] private ButtonView _targetButton;


    public override void OnStart(SceneGameActionContext context)
    {
        _isClicked = false;

        if (context.TargetRegistry == null) return;

        // 1. 在步驟開始時，直接去註冊事件
        RectTransform targetRect = context.TargetRegistry.GetTarget(TargetButtonID);
        if (targetRect != null)
        {
            _targetButton = targetRect.GetComponent<ButtonView>();
            if (_targetButton != null)
            {
                _targetButton.OnClicked += OnButtonClicked;
            }
            else
            {
                Debug.LogError($"[教學系統] 標靶 {TargetButtonID} 身上找不到 ButtonView 組件！");
            }
        }
        else
        {
            Debug.LogError($"[教學系統] 找不到 ID 為 {TargetButtonID} 的標靶！");
        }
    }

    public override bool IsConditionMet(SceneGameActionContext context)
    {
        // 2. 輪詢時只負責回傳狀態
        return _isClicked;
    }

    // 當 ButtonView 觸發 OnClicked 時執行
    private void OnButtonClicked()
    {
        _isClicked = true;
    }

    // 清除監聽與狀態
    public override void OnExit(SceneGameActionContext context)
    {
        // 3. 無論是過關還是中斷，Runner 都會呼叫這裡，確保事件被乾淨地移除
        if (_targetButton != null)
        {
            _targetButton.OnClicked -= OnButtonClicked;
            _targetButton = null;
        }
        _isClicked = false;
    }
}
