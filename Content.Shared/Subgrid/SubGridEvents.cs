using Content.Shared.GameCVars;

namespace Content.Shared.Subgrid;

/// <summary>
/// Event that is raised when <see cref="GameConfigVars.SubGridSize"/>
/// variable is changed for other systems to handle it.
/// </summary>
[ByRefEvent]
public record struct SubGridResizedEvent(int OldDivisions);

/// <summary>
/// Event that is raised when <see cref="GameConfigVars.SubGridHeight"/>
/// variable is changed for other systems to handle it.
/// </summary>
[ByRefEvent]
public record struct SubGridHeightChangedEvent();
