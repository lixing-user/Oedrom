using UnityEngine;

public class DynamicCaptureHandler : ICaptureHandler
{
    private readonly ITutorialOverrideService _overrideService;

    private readonly RemovalCaptureHandler _removalHandler;
    private readonly ShogiCaptureHandler _shogiHandler;

    public const string FeatureName_StandbyZone = "StandbyZone";

    public DynamicCaptureHandler(
        RemovalCaptureHandler removalHandler,
        ShogiCaptureHandler shogiHandler,
        ITutorialOverrideService overrideService)
    {
        _removalHandler = removalHandler;
        _shogiHandler = shogiHandler;
        _overrideService = overrideService;
    }


    public CaptureResult HandleCapture(ChessPiece capturedPiece, ChessPiece capturer, PiecePosition position)
    {
        bool isLocked = _overrideService.IsFeatureLocked(FeatureName_StandbyZone);

        // 根據解鎖狀態，決定使用哪一套規則
        if (!isLocked)
        {
            return _shogiHandler.HandleCapture(capturedPiece, capturer, position);
        }
        else
        {
            return _removalHandler.HandleCapture(capturedPiece, capturer, position);
        }
    }
}