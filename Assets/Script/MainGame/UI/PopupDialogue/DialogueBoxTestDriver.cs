using UnityEngine;
using Zenject;

public class DialogueBoxTestDriver : MonoBehaviour
{
    [Inject] private SceneGameActionContext _context;

    [InjectOptional] private IDialogueBoxManager _dialogueManager;

    [Header("直接指定測試資料")]
    [SerializeField] private ShowDialogueAction testAction;

    [Header("測試控制")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;


    void Update()
    {
        // 按空白鍵直接觸發，不走任何 Tutorial 流程
        if (Input.GetKeyDown(triggerKey))
        {
            testAction.Execute(_context);
            Debug.Log("Test action executed via DialogueBoxTestDriver");
        }
    }
}
