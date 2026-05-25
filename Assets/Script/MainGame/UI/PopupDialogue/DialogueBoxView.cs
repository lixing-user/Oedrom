using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class DialogueBoxView : MonoBehaviour
{
    #region Fields and Properties
    [Header("Layout")]
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text contentText;
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private RectTransform dialogueBoxRect;

    [Header("DialogueBoxPosition")]
    [SerializeField] private Vector2 _upperRight = new Vector2(0, 100);
    [SerializeField] private Vector2 _upperLeft = new Vector2(0, 100);
    [SerializeField] private Vector2 _buttomRight = new Vector2(0, 100);
    [SerializeField] private Vector2 _buttomLeft = new Vector2(0, 100);

    [Header("Animation")]
    [SerializeField] private float typingSpeed = 0.05f;

    public event Action OnConfirmRequested;
    #endregion

    #region Core Methods
    void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        // View 層偵測輸入，只負責轉發，不處理邏輯
        if (canvasGroup.alpha > 0 && Input.GetMouseButtonDown(0))
            OnConfirmRequested?.Invoke();
    }

    public void Show(Sprite avatar, string charName, string content, DialogueBoxAnchor anchor)
    {
        avatarImage.sprite = avatar;
        nameText.text = charName;
        contentText.text = content;
        SetAnchorPosition(anchor);
        dialogueContainer.SetActive(true);

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    #endregion

    #region Helper Methods
    private void SetAnchorPosition(DialogueBoxAnchor anchor)
    {
        Vector2 anchorPoint = anchor switch
        {
            DialogueBoxAnchor.BottomLeft => new Vector2(0, 0),
            DialogueBoxAnchor.BottomRight => new Vector2(1, 0),
            DialogueBoxAnchor.TopLeft => new Vector2(0, 1),
            DialogueBoxAnchor.TopRight => new Vector2(1, 1),
            _ => new Vector2(0.5f, 0.5f) // 預設居中
        };

        dialogueBoxRect.anchorMin = anchorPoint;
        dialogueBoxRect.anchorMax = anchorPoint;
        dialogueBoxRect.pivot = anchorPoint;

        dialogueBoxRect.anchoredPosition = anchor switch
        {
            DialogueBoxAnchor.BottomLeft => _buttomLeft,
            DialogueBoxAnchor.BottomRight => _buttomRight,
            DialogueBoxAnchor.TopLeft => _upperLeft,
            DialogueBoxAnchor.TopRight => _upperRight,
            _ => Vector2.zero
        };
    }
    #endregion
}
