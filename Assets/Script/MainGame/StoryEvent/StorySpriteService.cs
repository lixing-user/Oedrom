using System.Collections.Generic;
using UnityEngine;

public interface ISpriteService
{
    Sprite GetCharacterSprite(string characterName);
    Sprite GetBackgroundSprite(string backgroundName);
    Sprite GetCGSprite(string cgName);
    void LoadFromConfig(StoryChapterConfig config);
}

public class StorySpriteService : ISpriteService
{
    private Dictionary<string, Sprite> _characters = new();
    private Dictionary<string, Sprite> _backgrounds = new();
    private Dictionary<string, Sprite> _cgs = new();

    public void LoadFromConfig(StoryChapterConfig config)
    {
        _characters.Clear();
        _backgrounds.Clear();
        _cgs.Clear();

        foreach (var profile in config.characterProfiles)
        {
            Sprite portrait = profile.GetStorySprite();

            // 1. 註冊主名稱
            _characters[profile.characterName] = portrait;

            // 2. 註冊所有別名
            if (profile.aliases != null)
            {
                foreach (string alias in profile.aliases)
                {
                    if (!string.IsNullOrEmpty(alias))
                    {
                        _characters[alias] = portrait; // 讓別名也指向同一張圖
                    }
                }
            }
            Debug.Log($"[SpriteService] 載入角色: '{profile.characterName}' → {portrait?.name ?? "NULL"}");
        }
        foreach (var b in config.backgroundSprite) _backgrounds[b.backgroundName] = b.sprite;
        foreach (var cg in config.cgSprites) _cgs[cg.cgName] = cg.sprite;
    }

    public Sprite GetCharacterSprite(string name) => _characters.GetValueOrDefault(name);
    public Sprite GetBackgroundSprite(string name) => _backgrounds.GetValueOrDefault(name);
    public Sprite GetCGSprite(string name) => _cgs.GetValueOrDefault(name);
}
