using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BattleRewardView : MonoBehaviour
{
    [Inject] private RewardCardSlotPool _slotPool;

    [Header("General")]
    [SerializeField] private PanelView rewardPanel;
    [SerializeField] private ButtonView confirmRewardButton;

    [Header("Card Reward Slot")]
    [SerializeField] private Transform slotsParent;

    [Header("Morale Info")]
    [SerializeField] private GameObject moraleContainer;
    [SerializeField] private Image moraleBar;          // 計量條
    [SerializeField] private Image moralePreviewBar;
    [SerializeField] private TMP_Text moraleText;      // 顯示 "+X" 文字

    [Header("Supply Info")]
    [SerializeField] private GameObject supplyContainer;
    [SerializeField] private Image supplyBar;          // 計量條
    [SerializeField] private Image supplyPreviewBar;
    [SerializeField] private TMP_Text supplyText;      // 顯示 "+X" 文字

    [Header("Coin Info")]
    [SerializeField] private GameObject coinContainer;
    [SerializeField] private TMP_Text coinText;        // 顯示 "+X" 文字


    private List<RewardCardSlot> _activeSlots = new List<RewardCardSlot>();

    public event Action<PieceID, bool> OnSlotToggled;
    public event Action OnConfirmClicked;
    //public event Action OnBackClicked;


    private void Start()
    {
        confirmRewardButton.OnClicked += HandleConfirmClicked;
        //rewardPanel.OnBackRequested += () => OnBackClicked?.Invoke();
    }
    private void OnDestroy()
    {
        confirmRewardButton.OnClicked -= HandleConfirmClicked;
        //rewardPanel.OnBackRequested -= () => OnBackClicked?.Invoke();

        foreach (var slot in _activeSlots)
        {
            if (slot != null) // 防呆：以防 Unity 已經先一步把這個 Slot 給 Destroy 了
            {
                slot.OnToggleChanged -= HandleSlotToggled;
            }
        }
        _activeSlots.Clear();
    }

    private void HandleConfirmClicked() => OnConfirmClicked?.Invoke();


    #region Reward Slot System
    // 中繼方法：將 Slot 的事件往上傳遞給 Presenter
    private void HandleSlotToggled(PieceID id, bool state)
    {
        OnSlotToggled?.Invoke(id, state);
    }

    public void SetupRewardOptions(List<RewardUIData> rewards)
    {
        // 1. 每次 Setup 前，先將上次的 Slot 全數回收
        ClearSlots();

        // 2. 根據目前的戰利品數量，動態生成對應數量的 Slot
        foreach (var reward in rewards)
        {
            // 從池子裡拿一個出來
            RewardCardSlot slot = _slotPool.Spawn();

            // 設定父物件並確保縮放比例正確
            slot.transform.SetParent(slotsParent, false);

            // 填入資料
            slot.Setup(reward);

            // 拿出來的時候訂閱事件
            slot.OnToggleChanged += HandleSlotToggled;

            slot.Show();

            // 記錄下來方便之後回收
            _activeSlots.Add(slot);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotsParent as RectTransform);
    }

    private void ClearSlots()
    {
        foreach (var slot in _activeSlots)
        {
            // 回收前務必解除訂閱，避免 Memory Leak
            slot.OnToggleChanged -= HandleSlotToggled;
            _slotPool.Despawn(slot);
        }
        _activeSlots.Clear();
    }

    public Vector3 GetSlotPosition(PieceID id)
    {
        // 實際開發中可透過 Dictionary 儲存 Slot 引用以優化查找
        return transform.position;
    }
    #endregion

    #region Resource Reward UI System
    public void SetupCoinRewards(ResourceRewardData data)
    {
        //bool hasAnyResource = data.MoraleAmount > 0 || data.SupplyAmount > 0 || data.CoinAmount > 0;

        //if (!hasAnyResource) return;

        // --- 設定 Coin (純文字) ---
        if (data.CoinAmount > 0)
        {
            coinContainer.SetActive(true);
            coinText.text = $"+ {Mathf.RoundToInt(data.CoinAmount)}";
        }
        else
        {
            coinContainer.SetActive(false);
        }
    }

    #region Supply 顯示邏輯
    public void SetSupplyDisplay(float currentSupply, float addedSupply, float maxSupply)
    {
        if (addedSupply <= 0)
        {
            supplyContainer.SetActive(false);
            return;
        }
        supplyContainer.SetActive(true);

        supplyText.text = $"+ {Mathf.RoundToInt(addedSupply)}";
        supplyBar.fillAmount = Mathf.Clamp01(currentSupply / maxSupply);
    }

    public Tween PlaySupplyFillAnimation(float startValue, float targetValue, float maxSupply, float duration = 0.5f)
    {
        float targetFill = Mathf.Clamp01(targetValue / maxSupply);
        // 使用 DOFillAmount 平滑過渡，並加上 OutQuad 緩動曲線讓視覺更自然
        return supplyBar.DOFillAmount(targetFill, duration).SetEase(Ease.OutQuad);
    }
    #endregion

    #region Morale 顯示邏輯
    public void SetupMoraleDisplay(float currentMorale, float addedMorale, float maxMorale)
    {
        if (addedMorale <= 0)
        {
            moraleContainer.SetActive(false);
            return;
        }

        moraleContainer.SetActive(true);
        moraleText.text = $"+ {Mathf.RoundToInt(addedMorale)}";

        // 設定表層為當前值
        moraleBar.fillAmount = Mathf.Clamp01(currentMorale / maxMorale);
        // 設定底層為「加上獎勵後的預期值」
        moralePreviewBar.fillAmount = Mathf.Clamp01((currentMorale + addedMorale) / maxMorale);
    }

    // 2. 執行領取動畫 (將表層的 Bar 填滿至底層的長度)
    public Tween PlayMoraleFillAnimation(float startValue, float targetValue, float maxMorale, float duration = 0.5f)
    {
        float targetFill = Mathf.Clamp01(targetValue / maxMorale);
        return moraleBar.DOFillAmount(targetFill, duration).SetEase(Ease.OutQuad);
    }
    #endregion

    #endregion

    #region Reward Panel
    public void ShowRewardPanel()
    {
        rewardPanel.Show();
    }
    public void HideRewardPanel()
    {
        rewardPanel.Hide();
    }
    #endregion
}
