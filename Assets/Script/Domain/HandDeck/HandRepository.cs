using ChessPieceCardProduction;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface IHandDeckRepository
{
    HandDeck Load();
    void Save(HandDeck handDeck);
    bool Exists();
    void Delete();
}

// Infrastructure/Persistence/Repositories/HandDeckRepository.cs （JSON 文件存儲）
public class HandDeckRepository : IHandDeckRepository
{
    private readonly string _saveFilePath;
    private const string SAVE_FILE_NAME = "handdeck_save.json";

    public HandDeckRepository()
    {
        _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        Debug.Log($"HandDeckRepository 初始化，存檔路徑：{_saveFilePath}");
    }


    #region 業務邏輯
    public HandDeck Load()
    {
        if (!Exists())
        {
            Debug.LogWarning("存檔不存在，返回預設 HandDeck");
            return CreateDefault();
        }

        try
        {
            string json = File.ReadAllText(_saveFilePath);
            var dto = JsonUtility.FromJson<HandDeckDTO>(json);

            if (dto == null)
            {
                Debug.LogError("JSON 反序列化失敗");
                return CreateDefault();
            }

            Debug.Log($"成功載入 HandDeck，包含 {dto.cards.Count} 張卡牌");
            return dto.ToDomain();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"載入 HandDeck 失敗：{ex.Message}\n{ex.StackTrace}");
            return CreateDefault();
        }
    }
   

    public void Save(HandDeck handDeck)
    {
        if (handDeck == null)
        {
            Debug.LogError("嘗試儲存 null HandDeck");
            return;
        }

        try
        {
            var dto = new HandDeckDTO(handDeck);
            string json = JsonUtility.ToJson(dto, prettyPrint: true);

            // 確保目錄存在
            string directory = Path.GetDirectoryName(_saveFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_saveFilePath, json);
            Debug.Log($"HandDeck 已儲存，包含 {handDeck.Cards.Count} 張卡牌");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"儲存 HandDeck 失敗：{ex.Message}\n{ex.StackTrace}");
        }
    }
  

    public bool Exists()
    {
        return File.Exists(_saveFilePath);
    }


    public void Delete()
    {
        if (Exists())
        {
            File.Delete(_saveFilePath);
            Debug.Log("HandDeck 存檔已刪除");
        }
    }
    #endregion

    private HandDeck CreateDefault()
    {
        return new HandDeck(maxSize: 10); // 預設手牌上限 10 張
    }
}