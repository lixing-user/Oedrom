using UnityEngine;

// 職責：專門暫存準備執行、但需要等待場景就緒的教學序列
public class PendingTutorialContext
{
    public string SequenceId { get; private set; }
    public TutorialStep[] Steps { get; private set; }

    public bool HasPending => !string.IsNullOrEmpty(SequenceId);

    // 存入預定事項
    public void SetPendingSequence(string id, TutorialStep[] steps)
    {
        SequenceId = id;
        this.Steps = steps;
    }

    // 執行完畢後清空
    public void Consume()
    {
        SequenceId = null;
        Steps = null;
    }
}
