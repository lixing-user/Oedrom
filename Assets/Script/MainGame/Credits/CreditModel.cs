using System;
using System.Collections.Generic;
using UnityEngine;

public enum CreditDisplayType { Slide, Scroll }

public struct CreditSection
{
    public CreditDisplayType DisplayType;
    public string Content;
    public int PanelIndex;
}

public class CreditModel
{
    public List<CreditSection> Sections { get; private set; } = new List<CreditSection>();

    // 透過建構子直接載入 TXT 並解析
    public CreditModel(TextAsset textAsset)
    {
        ParseText(textAsset.text);
    }

    private void ParseText(string rawText)
    {
        string[] blocks = rawText.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var block in blocks)
        {
            var section = new CreditSection();
            string content = block.Trim();

            if (content.StartsWith("[ShowPanel:"))
            {
                section.DisplayType = CreditDisplayType.Slide;

                // 取出 "0", "1" 等數字
                string indexStr = content.Replace("[ShowPanel:", "").Replace("]", "").Trim();
                if (int.TryParse(indexStr, out int index))
                {
                    section.PanelIndex = index;
                }
            }
            else if (content.StartsWith("[Scroll]"))
            {
                section.DisplayType = CreditDisplayType.Scroll;
                section.Content = content.Replace("[Scroll]", "").Trim();
            }

            Sections.Add(section);
        }
    }
}
