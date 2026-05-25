using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


// Provider , 解析 CSV 和提供對話資料
public interface IDialogueService
{
    void LoadDialogue(TextAsset csvFile);
    DialogueLine GetDialogueById(int id);
    bool IsDialogueEnd(int id);
}


public class DialogueService : IDialogueService
{
    private Dictionary<int, DialogueLine> _dialogueDatabase = new Dictionary<int, DialogueLine>();
    private string[] _rawRows;


    public void LoadDialogue(TextAsset csvFile)
    {
        _dialogueDatabase.Clear();

        string rawText = csvFile.text.TrimStart('\uFEFF');
        ParseDialogueData(rawText);

        Debug.Log($"[DialogueService] 載入 {_dialogueDatabase.Count} 行對話");
    }

    private void ParseDialogueData(string rawText)
    {
        string[] rows = rawText.Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (string row in rows.Skip(1))
        {
            string[] cells = row.Split(',');
            if (cells.Length < 2) continue;

            string type = cells[0];
            if (!int.TryParse(cells[1].Trim(), out int id)) // TryParse 防禦
            {
                Debug.LogWarning($"[DialogueService] 無法解析 ID：'{cells[1]}' | 行：{row}");
                continue;
            }

            var line = new DialogueLine
            {
                id = id,
                type = type
            };


            switch (type)
            {
                case "#": // 普通對話
                    line.characterName = cells[2];
                    line.position = cells[3];
                    line.charPos = ParsePosition(cells[3].Trim());
                    line.content = cells[4];
                    line.nextId = int.Parse(cells[5]);
                    line.effectTag = cells[6].Trim();
                    break;

                case "CG": // 特殊CG
                case "BG": // 背景切換
                    line.content = cells[4]; // 資源名稱
                    line.nextId = cells.Length > 5 && !string.IsNullOrEmpty(cells[5])
                        ? int.Parse(cells[5])
                        : id + 1;
                    break;

                case "END": // 結束
                    // END 不需要額外欄位
                    break;

                default:
                    Debug.LogWarning($"[DialogueService] 未知類型: {type}");
                    continue;
            }
            _dialogueDatabase[id] = line;
        }
    }

    private CharacterSpritePosition ParsePosition(string raw)
    {
        return raw.ToLower() switch
        {
            "left" => CharacterSpritePosition.Left,
            "right" => CharacterSpritePosition.Right,
            "center" => CharacterSpritePosition.Center,
            "" => CharacterSpritePosition.None,
            _ => CharacterSpritePosition.None
        };
    }


    #region Helper Methods
    public DialogueLine GetDialogueById(int id)
    {
        return _dialogueDatabase.ContainsKey(id) ? _dialogueDatabase[id] : null;
    }

    public bool IsDialogueEnd(int id)
    {
        var line = GetDialogueById(id);
        return line != null && line.IsEnd;
    }
    #endregion
}
