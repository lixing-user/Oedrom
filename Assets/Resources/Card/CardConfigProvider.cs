using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public interface ICardConfigProvider
{
    CardConfig GetConfig(ChessPieceType type);
    bool TryGetConfig(ChessPieceType type, out CardConfig config);
}

public class CardConfigProvider : ICardConfigProvider
{
    private readonly Dictionary<ChessPieceType, CardConfig> _configLookup;

    [Inject]
    public CardConfigProvider(CardConfig[] allConfigs)
    {
        // 初始化時建立 Dictionary，之後查詢是 O(1)
        _configLookup = new Dictionary<ChessPieceType, CardConfig>();

        foreach (var config in allConfigs)
        {
            if (config == null)
            {
                Debug.LogError("CardConfig 陣列中存在 null 元素");
                continue;
            }

            if (_configLookup.ContainsKey(config.type))
            {
                Debug.LogError($"重複的卡牌類型配置: {config.type}");
                continue;
            }

            _configLookup[config.type] = config;
        }

        Debug.Log($"CardConfigProvider 初始化完成，載入了 {_configLookup.Count} 個配置");
    }

    public CardConfig GetConfig(ChessPieceType type)
    {
        if (_configLookup.TryGetValue(type, out var config))
        {
            return config;
        }

        Debug.LogError($"找不到類型 {type} 的卡牌配置。可用類型: {string.Join(", ", _configLookup.Keys)}");
        return null;
    }

    // 更安全的 TryGet 模式
    public bool TryGetConfig(ChessPieceType type, out CardConfig config)
    {
        return _configLookup.TryGetValue(type, out config);
    }
}

#region 窄介面，只暴露 Sprite
public interface IPieceIconResolver
{
    Sprite GetIcon(ChessPieceType type);
}

public class PieceIconResolver : IPieceIconResolver
{
    private readonly ICardConfigProvider _provider;
    public PieceIconResolver(ICardConfigProvider provider) => _provider = provider;

    public Sprite GetIcon(ChessPieceType type)
    {
        if (_provider.TryGetConfig(type, out var c) && c.defaultIcon != null)
            return c.defaultIcon;

        Debug.LogWarning($"[PieceIconResolver] {type} 沒有可用的 defaultIcon");
        return null;
    }
}
#endregion