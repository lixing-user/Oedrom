using ChessPieceCardProduction;
using System.Collections.Generic;
using UnityEngine;


// Infrastructure/Card/CardStaticDataRegistry.cs

public class CardStaticDataRegistry
{
    private readonly Dictionary<ChessPieceType, CardConfig> _cardTemplates;

    public CardStaticDataRegistry()
    {
        _cardTemplates = new Dictionary<ChessPieceType, CardConfig>();
        LoadCardTemplates();
    }


    private void LoadCardTemplates()
    {
        CardConfig[] templates = Resources.LoadAll<CardConfig>("Card/Data");
        Debug.Log($"成功載入 {templates.Length} 個卡牌模板");

        foreach (var template in templates)
        {
            if (template != null)
            {
                Debug.Log($"載入卡牌：名稱={template.name}, 類型={template.type}, Cost={template.baseCost}");
                if (_cardTemplates.ContainsKey(template.type))
                {
                    Debug.LogWarning($"重複的卡牌類型 {template.type}，已覆蓋！");
                }

                _cardTemplates[template.type] = template;
            }
            else
            {
                Debug.Log("載入的卡牌模板為空！");
            }
        }
        Debug.Log($"最終字典包含 {_cardTemplates.Count} 個條目");
    }


    // 查詢方法（不創建 Domain Object）
    public CardConfig GetTemplate(ChessPieceType type)
    {
        if (_cardTemplates.TryGetValue(type, out var template))
            Debug.Log($"找到類型 {type} 的卡牌模板：名稱={template.name}, Cost={template.baseCost}");
        else
            Debug.Log($"找不到類型 {type} 的卡牌模板！");
        return template;
    }

    public int GetBaseCost(ChessPieceType type)
    {
        var template = GetTemplate(type);
        return template?.baseCost ?? 0;
    }

    public Sprite GetIcon(ChessPieceType type)
    {
        var template = GetTemplate(type);
        return template?.defaultIcon;
    }
}