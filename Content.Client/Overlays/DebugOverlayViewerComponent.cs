namespace Content.Client.Overlays;

/// <summary>
/// Controls visibility of most debug overlays.
/// </summary>
[RegisterComponent]
public sealed partial class DebugOverlayViewerComponent : Component
{
    [DataField]
    public bool SubGridChunkOverlay;

    [DataField]
    public bool SubGridTilesOverlay;
}
