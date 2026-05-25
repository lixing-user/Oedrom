using UnityEngine;

// Domain/Models/RunProgress.cs
public class RunProgress
{
    public int CompletedBattles { get; private set; }

    public void RecordBattleCompleted()
    {
        CompletedBattles++;
    }

    public void ResetProgress()
    {
        CompletedBattles = 0;
    }
}
