using System;
using UnityEngine;
using Zenject;

public enum VariableType
{
    morale,
    popularity,
    supply,
    coin
}

public interface IResourceManager
{
    // === 通用方法 ===
    void InitializeVariables();
    void UpdateVariable(VariableType type, float amount);
    float GetVariable(VariableType type);
    float GetMaxValue(VariableType type);
    int GetDynamicPrice(int basePrice);

    // === 補給業務 ===
    void ApplyTurnStartEffects();
    void ReplenishSupply(float amount);
    bool DeductSupply(int amount);
    bool CanAffordSupply(int amount);

    // === 金幣業務 ===
    bool TryPurchaseItem(int itemPrice);
    void RewardCoin(float amount); // 戰鬥勝利、賣出卡牌獲得金幣
    bool TrySpendCoin(float amount); // 其他花費（如修復卡牌）

    // === 軍心業務 ===
    void IncreaseeMorale(float amount); // 提升軍心（如事件選項）
    void DecreaseMorale(float amount); // 降低軍心（如搶劫失敗、補給耗盡）

    // === 民心業務 ===
    void IncreasePopularity(float amount);
    void DecreasePopularity(float amount);
}

//管理資源（morale、popularity、supply、coin）
//public class PlayerResourceManager : IResourceManager,IInitializable
//{
//    [Inject(Optional = true)] // 若有多個場景組，可設為Optional
//    private SignalBus signalBus;


//    // 玩家資源
//    private float morale = 5f ;
//    private float popularity = 5f;
//    private float supply = 500f;
//    private float coin = 5000f;

//    public readonly float maxMorale = 10f;
//    public readonly float maxPopularity = 10f;
//    public readonly float maxSupply = 500f;
//    public readonly float maxCoin = 10000f;
//    private readonly float minValue = 0f;

//    public float Morale { get => morale; set => morale = value; }
//    public float Popularity { get => popularity; set => popularity = value; }
//    public float Supply { get => supply; set => supply = value; }
//    public float Coin { get => coin; set => coin = value; }

//    [Inject]
//    public void Construct(SignalBus signalBus)
//    {
//        this.signalBus = signalBus;
//    }
//    public void Initialize()
//    {
//        signalBus.Subscribe<MainGameTurnChangedSignal>(OnTurnChanged);
//        InitializeVariables();
//    }


//    // 資源操作
//    public void InitializeVariables()
//    {
//        morale = 5f;
//        popularity = 5f;
//        supply = 500f;
//        coin = 5000f;
//        // 初始化時通知 UI 更新
//        signalBus.Fire(new ResourceChangedSignal(VariableType.morale, morale));
//        signalBus.Fire(new ResourceChangedSignal(VariableType.popularity, popularity));
//        signalBus.Fire(new ResourceChangedSignal(VariableType.supply, supply));
//        signalBus.Fire(new ResourceChangedSignal(VariableType.coin, coin));
//    }
//    public void UpdateVariable(VariableType type, float amount)
//    {
//        switch (type)
//        {
//            case VariableType.morale:
//                morale = Mathf.Clamp(morale + amount, minValue, maxMorale);
//                signalBus.Fire(new ResourceChangedSignal(type, morale));
//                break;
//            case VariableType.popularity:
//                popularity = Mathf.Clamp(popularity + amount, minValue, maxPopularity);
//                signalBus.Fire(new ResourceChangedSignal(type, popularity));
//                signalBus.Fire(new EnemyDifficultyDeterminedSignal((int)popularity));
//                break;
//            case VariableType.supply:
//                supply = Mathf.Clamp(supply + amount, minValue, maxSupply);
//                signalBus.Fire(new ResourceChangedSignal(type, supply));
//                break;
//            case VariableType.coin:
//                coin = Mathf.Clamp(coin + amount, minValue, maxCoin);
//                signalBus.Fire(new ResourceChangedSignal(type, coin));
//                break;
//        }
//    }

//    public float GetVariable(VariableType type)
//    {
//        switch (type)
//        {
//            case VariableType.morale: return morale;
//            case VariableType.popularity: return popularity;
//            case VariableType.supply: return supply;
//            case VariableType.coin: return coin;
//            default: return 0f;
//        }
//    }

//    // 根據人氣計算動態價格(ShopUI)
//    public int GetDynamicPrice(int basePrice)
//    {
//        float popularity = GetVariable(VariableType.popularity);
//        float discount = Mathf.Clamp(1f - 0.05f * popularity, 0.5f, 1f);
//        return Mathf.RoundToInt(basePrice * discount);
//    }

//    private void OnTurnChanged(MainGameTurnChangedSignal signal)
//    {
//        if (signal.IsWhiteTurn)
//        {
//            UpdateVariable(VariableType.supply, 100f);  // 自動增加補給，內部發射 ResourceChangedSignal 更新 UI
//        }
//    }
//}



