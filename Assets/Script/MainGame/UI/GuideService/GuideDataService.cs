using System.Collections.Generic;
using UnityEngine;

public struct GuideTabDisplayData
{
    public string Name;
    public Sprite Icon;
}


public interface IGuideDataService
{
    void LoadAllGuides(); // 如果使用 Addressables 可以做成非同步
    List<string> GetAllChapterNames();
    List<GuideTabDisplayData> GetTabDisplayDatas();
    GuideChapterSO GetChapter(int index);
    int GetChapterCount();
}

public class GuideDataService : IGuideDataService
{
    private List<GuideChapterSO> _chapters = new List<GuideChapterSO>();

    public void LoadAllGuides()
    {
        // 假設使用 Resources 載入
        var loadedChapters = Resources.LoadAll<GuideChapterSO>("GuideData");
        _chapters.AddRange(loadedChapters);
        Debug.Log($"[GuideDataService] 載入了 {_chapters.Count} 個引導章節");
    }

    public List<string> GetAllChapterNames()
    {
        List<string> names = new List<string>();
        foreach (var ch in _chapters) names.Add(ch.chapterName);
        return names;
    }

    public List<GuideTabDisplayData> GetTabDisplayDatas()
    {
        var list = new List<GuideTabDisplayData>();
        foreach (var ch in _chapters)
        {
            list.Add(new GuideTabDisplayData { Name = ch.chapterName, Icon = ch.chapterIcon });
        }
        return list;
    }

    public GuideChapterSO GetChapter(int index)
    {
        if (index >= 0 && index < _chapters.Count) return _chapters[index];
        return null;
    }

    public int GetChapterCount() => _chapters.Count;
}
