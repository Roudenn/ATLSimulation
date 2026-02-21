using Content.Shared.Atmospherics;
using Content.Shared.Temperature;
using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class SubGridChunkComponent : Component
{
    // TODO: remove this shitcode when a method to parent stuff to grids even when they are off grid gets added
    [DataField, AutoNetworkedField]
    public EntityUid ParentGrid;

    [DataField, AutoNetworkedField]
    public Vector2i ChunkIndices;

    [DataField, AutoNetworkedField]
    public TileTemperature[] TemperatureMap;

    [DataField, AutoNetworkedField]
    public TileAtmosphere[] AtmosphereMap;
}
