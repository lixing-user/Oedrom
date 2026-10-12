using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class BattleRewardPresenter : MonoBehaviour
{
    [Inject] private SignalBus _signalBus;
    [Inject] private IResourceManager _resourceManager;
    [Inject] private IRewardService _rewardService;
    [Inject] private IPieceIconResolver _iconResolver; 

    private BattleRewardView _view;
    private List<PieceID> _selectedIds = new List<PieceID>();

    private ResourceRewardData _currentResourceData;


    void Start()
    {
        _view = GetComponent<BattleRewardView>();

        _view.OnSlotToggled += OnSlotToggled;
        _view.OnConfirmClicked += OnConfirmCollect;
        //_view.OnBackClicked += OnBackRequested;

        _signalBus.Subscribe<ShowRewardRequestedSignal>(OnShowRewardRequested);
    }
    void OnDestroy()
    {
        _view.OnSlotToggled -= OnSlotToggled;
        _view.OnConfirmClicked -= OnConfirmCollect;
        //_view.OnBackClicked -= OnBackRequested;

        _signalBus.Unsubscribe<ShowRewardRequestedSignal>(OnShowRewardRequested);
    }

    #region 核心功能
    private void OnShowRewardRequested(ShowRewardRequestedSignal signal)
    {
        _selectedIds.Clear();

        // 1. 卡牌獎勵：Domain → UI DTO，只在這裡做一次
        var uiData = _rewardService.GetAvailableRewards()
            .Select(piece => new RewardUIData
            {
                Id = piece.Id,
                Type = piece.Type,
                DisplayName = piece.Type.ToString(),
                Icon = _iconResolver.GetIcon(piece.Type),
                IsBroken = piece.IsBroken,
                SupplyCost = 0
            })
            .ToList();

        _view.SetupRewardOptions(uiData);

        // 2. 處理基礎資源獎勵
        _currentResourceData = _rewardService.GenerateResourceRewards();

        // 取得當前資源狀態
        float currentMorale = _resourceManager.GetVariable(VariableType.morale);
        float maxMorale = _resourceManager.GetMaxValue(VariableType.morale);
        float currentSupply = _resourceManager.GetVariable(VariableType.supply);
        float maxSupply = _resourceManager.GetMaxValue(VariableType.supply);

        // 設定 View 的初值與預覽值
        _view.SetupMoraleDisplay(currentMorale, _currentResourceData.MoraleAmount, maxMorale);
        _view.SetSupplyDisplay(currentSupply, _currentResourceData.SupplyAmount, maxSupply);
        _view.SetupCoinRewards(_currentResourceData);

        // 3. 顯示面板
        _view.ShowRewardPanel();

        _view.ShowRewardPanel();
        StartCoroutine(CollectResourcesCoroutine(currentMorale, currentSupply));

        if (uiData.Count == 0)
        {
            Debug.Log("[BattleReward] 卡牌獎勵庫為空，僅派發資源獎勵");
        }
    }

    private void OnSlotToggled(PieceID id, bool isSelected)
    {
        if (isSelected)
        {
            if (!_selectedIds.Contains(id)) _selectedIds.Add(id);
        }
        else
        {
            _selectedIds.Remove(id);
        }
    }

    // 當玩家點擊 UI 上的「確認領取」大按鈕時
    public void OnConfirmCollect()
    {
        if (_selectedIds.Count > 0)
        {
            foreach (var id in _selectedIds)
            {
                _rewardService.ClaimCardReward(id);

                // 執行 3D 飛向 UI 的動畫
                // Vector3 targetPos = _view.GetSlotPosition(id);
                // _signalBus.Fire(new PieceFlyToUISignal(id, targetPos));
            }
        }

        // 收尾與關閉面板
        _selectedIds.Clear();
        _view.HideRewardPanel();

        Debug.Log("[BattleReward] 獎勵結算完成並關閉面板");
    }

    private IEnumerator CollectResourcesCoroutine(float oldMorale, float oldSupply)
    {
        yield return new WaitForSeconds(0.5f);

        _rewardService.ClaimResourceRewards();

        Sequence animSequence = DOTween.Sequence();

        if (_currentResourceData.MoraleAmount > 0)
        {
            float maxMorale = _resourceManager.GetMaxValue(VariableType.morale);
            animSequence.Join(_view.PlayMoraleFillAnimation(oldMorale, oldMorale + _currentResourceData.MoraleAmount, maxMorale));
        }

        if (_currentResourceData.SupplyAmount > 0)
        {
            float maxSupply = _resourceManager.GetMaxValue(VariableType.supply);
            animSequence.Join(_view.PlaySupplyFillAnimation(oldSupply, oldSupply + _currentResourceData.SupplyAmount, maxSupply));
        }

        // 等待動畫序列播放完畢 (此時玩家可能正在思考要選哪張卡)
        if (animSequence.Duration() > 0)
        {
            yield return animSequence.WaitForCompletion();
        }
    }

    //private void OnBackRequested()
    //{
    //    _selectedIds.Clear();
    //    _view.HideRewardPanel();
    //}
    #endregion
}
