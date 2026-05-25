using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class RaycastDebugger : MonoBehaviour
{
    private GraphicRaycaster raycaster;
    private EventSystem eventSystem;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponent<Canvas>();
        raycaster = GetComponent<GraphicRaycaster>();
        eventSystem = EventSystem.current;

        Debug.Log("===== Canvas_Camera 初始化檢查 =====");
        Debug.Log($"Canvas: {canvas != null}");
        if (canvas != null)
        {
            Debug.Log($"  Render Mode: {canvas.renderMode}");
            Debug.Log($"  World Camera: {canvas.worldCamera?.name ?? "NULL"}");
        }

        Debug.Log($"GraphicRaycaster: {raycaster != null} (enabled: {raycaster?.enabled})");
        Debug.Log($"EventSystem: {eventSystem != null}");

      
    }

    private void Update()
    {
        // 只在點擊時執行，不是每幀都執行
        if (Input.GetMouseButtonDown(0))
        {
            TestRaycast();
        }
    }

    private void TestRaycast()
    {
        //Debug.Log($"========== 點擊測試 ==========");
        //Debug.Log($"滑鼠位置: {Input.mousePosition}");

        if (raycaster == null || eventSystem == null)
        {
            Debug.LogError("Raycaster 或 EventSystem 為 null!");
            return;
        }

        // 列出所有卡牌
        var cards = GetComponentsInChildren<CardInputInspector>(true);
        //Debug.Log($"Canvas 下的卡牌數量: {cards.Length}");
        foreach (var card in cards)
        {
            //Debug.Log($"  - {card.gameObject.name} (active: {card.gameObject.activeInHierarchy}, enabled: {card.enabled})");
        }
        //Debug.Log("===================================");

        PointerEventData pointerData = new PointerEventData(eventSystem)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerData, results);

        Debug.Log($"擊中 UI 數量: {results.Count}");

        if (results.Count == 0)
        {
            //Debug.LogWarning("沒有擊中任何 UI!");

            // 手動檢查所有卡牌
            var allCards = GetComponentsInChildren<CardInputInspector>(true);
            Debug.Log($"--- 手動檢查 {allCards.Length} 張卡牌 ---");

            foreach (var card in allCards)
            {
                if (!card.gameObject.activeInHierarchy) continue;

                var rectTransform = card.GetComponent<RectTransform>();
                if (rectTransform == null) continue;

                Vector2 localPoint;
                Camera cam = canvas.worldCamera;

                bool isInRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    rectTransform,
                    Input.mousePosition,
                    cam,
                    out localPoint
                );

                if (isInRect && rectTransform.rect.Contains(localPoint))
                {
                    Debug.Log($"   滑鼠在卡牌內: {card.gameObject.name}");
                    Debug.Log($"   本地座標: {localPoint}");
                    Debug.Log($"   世界座標: {rectTransform.position}");

                    var image = card.GetComponent<Image>();
                    if (image != null)
                    {
                        Debug.Log($"   Image.raycastTarget: {image.raycastTarget}");
                        Debug.Log($"   Image.enabled: {image.enabled}");
                    }
                    else
                    {
                        Debug.LogError("   沒有 Image 組件!");
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                Debug.Log($"[{i}] {result.gameObject.name}");

                var cardInput = result.gameObject.GetComponent<CardInputInspector>();
                if (cardInput != null)
                {
                    Debug.Log($"    有 CardInputInspector");
                }
            }
        }

        Debug.Log($"==============================");
    }
}
