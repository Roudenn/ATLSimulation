using Content.Shared.Constants;
using Content.Shared.GameCVars;
using Robust.Shared.GameStates;

namespace Content.Shared.Temperature.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TemperatureContainerComponent : Component
{
    /// <summary>
    /// The main array that stores all tiles. Has a size of <see cref="GameConfigVars.SubGridSize"/>^2.
    /// Stored only for transportation purposes when the entity gets unanchored: in that case it's handled differently.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public TileTemperature[]? ContainedTiles;
    
    /// <summary>
    /// Starting temperature of all tiles inside the entity.
    /// </summary>
    [DataField]
    public float StartingTemperature = PhysicalConstants.ROOM_TEMPERATURE;
}
