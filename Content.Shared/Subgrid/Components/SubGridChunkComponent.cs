using Content.Shared.Atmospherics;
using Content.Shared.Temperature;
using Robust.Shared.GameStates;

namespace Content.Shared.Subgrid.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class SubGridChunkComponent : Component
{
    [DataField, AutoNetworkedField] 
    public TileTemperature[] TemperatureMap;
    
    [DataField, AutoNetworkedField]
    public TileAtmosphere[] AtmosphereMap;
}
