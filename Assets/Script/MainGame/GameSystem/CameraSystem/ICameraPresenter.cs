using UnityEngine;

public interface ICameraPresenter
{
    void PlayOpeningSequence();
    void RequestDefaultMode(bool force = false);
    void RequestTopViewMode();
    void RequestFreelookMode();

    void RequestPieceFocusMode(Transform target);
    void RequestEnemyFocusMode(Transform target);
    void RequestStandbyZoneMode();

    void LockCamera(string requesterId, bool isLocked);
    bool IsLocked { get; }

    CameraMode CurrentMode { get; }
}
