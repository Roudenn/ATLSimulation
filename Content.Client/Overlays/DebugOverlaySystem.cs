using Content.Client.Atmospherics.Overlays;
using Content.Client.Subgrid.Overlays;
using Content.Client.Temperature.Overlays;
using Content.Shared.Atmospherics.Factory;
using Content.Shared.Subgrid.Systems;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Overlays;

// TODO this is horrible
public sealed class DebugOverlaySystem : EntitySystem
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedSubGridSystem _subGrid = default!;
    [Dependency] private readonly GasMixtureFactory _gasManager = default!;

    private SubGridChunkOverlay _subGridChunkOverlay = default!;
    private SubGridTileOverlay _subGridTileOverlay = default!;
    private HeatMapOverlay _heatMapOverlay = default!;
    private AtmosCompositionOverlay _atmosCompositionOverlay = default!;
    private AtmosPressureOverlay _atmosPressureOverlay = default!;
    private AtmosHeatMapOverlay _atmosHeatMapOverlay = default!;
    private InternalEnergyOverlay _internalEnergyOverlay = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DebugOverlayViewerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DebugOverlayViewerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<DebugOverlayViewerComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<DebugOverlayViewerComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _subGridChunkOverlay = new(EntityManager, _eyeManager, _xform, _subGrid);
        _subGridTileOverlay = new(EntityManager, _eyeManager, _xform, _subGrid);
        _heatMapOverlay = new(EntityManager, _eyeManager, _xform, _subGrid);
        _atmosCompositionOverlay = new(EntityManager, _eyeManager, _gasManager, _xform, _subGrid);
        _atmosPressureOverlay = new(EntityManager, _eyeManager, _gasManager, _xform, _subGrid);
        _atmosHeatMapOverlay = new(EntityManager, _eyeManager, _xform, _subGrid);
        _internalEnergyOverlay = new(EntityManager, _eyeManager, _xform, _subGrid);
    }

    public void UpdateOverlays(Entity<DebugOverlayViewerComponent?> ent)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        RemoveOverlays();
        AddOverlays(ent!);
    }

    private void OnPlayerAttached(Entity<DebugOverlayViewerComponent> ent, ref LocalPlayerAttachedEvent args)
    {
        AddOverlays(ent);
    }

    private void OnPlayerDetached(Entity<DebugOverlayViewerComponent> ent, ref LocalPlayerDetachedEvent args)
    {
        RemoveOverlays();
    }

    private void OnInit(Entity<DebugOverlayViewerComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent)
            return;

        AddOverlays(ent);
    }

    private void OnShutdown(Entity<DebugOverlayViewerComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent)
            return;

        RemoveOverlays();
    }

    private void AddOverlays(Entity<DebugOverlayViewerComponent> ent)
    {
        if (ent.Comp.SubGridChunkOverlay)
            _overlayMan.AddOverlay(_subGridChunkOverlay);
        if (ent.Comp.SubGridTilesOverlay)
            _overlayMan.AddOverlay(_subGridTileOverlay);
        if (ent.Comp.HeatMapOverlay)
            _overlayMan.AddOverlay(_heatMapOverlay);
        if (ent.Comp.AtmosCompositionOverlay)
            _overlayMan.AddOverlay(_atmosCompositionOverlay);
        if (ent.Comp.AtmosPressureOverlay)
            _overlayMan.AddOverlay(_atmosPressureOverlay);
        if (ent.Comp.AtmosTemperatureOverlay)
            _overlayMan.AddOverlay(_atmosHeatMapOverlay);
        if (ent.Comp.InternalEnergyOverlay)
            _overlayMan.AddOverlay(_internalEnergyOverlay);
    }

    private void RemoveOverlays()
    {
        _overlayMan.RemoveOverlay(_subGridChunkOverlay);
        _overlayMan.RemoveOverlay(_subGridTileOverlay);
        _overlayMan.RemoveOverlay(_heatMapOverlay);
        _overlayMan.RemoveOverlay(_atmosCompositionOverlay);
        _overlayMan.RemoveOverlay(_atmosPressureOverlay);
        _overlayMan.RemoveOverlay(_atmosHeatMapOverlay);
        _overlayMan.RemoveOverlay(_internalEnergyOverlay);
    }
}
