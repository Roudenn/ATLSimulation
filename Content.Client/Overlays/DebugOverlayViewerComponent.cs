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

    [DataField]
    public bool HeatMapOverlay;

    [DataField]
    public bool InternalEnergyOverlay;

    [DataField]
    public bool AtmosCompositionOverlay;

    [DataField]
    public bool AtmosPressureOverlay;

    [DataField]
    public bool AtmosTemperatureOverlay;
}
