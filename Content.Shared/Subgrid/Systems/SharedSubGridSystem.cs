using Content.Shared.Constants;
using Content.Shared.GameCVars;
using Content.Shared.Subgrid.Components;
using Robust.Shared.Configuration;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager CfgManager = default!;
    
    /// <summary>
    /// Amount of subdivisions of a single grid tile.
    /// </summary>
    public int SubGridSize { get; private set; } = 1;
    
    /// <summary>
    /// Total amount of tiles in a single subgrid chunk.
    /// </summary>
    public int SubGridChunkArea { get; private set; } = SystemConstants.PvsChunkSize * 2;
    
    private EntityQuery<SubGridChunkComponent> _chunkQuery;
    
    public static Vector2i[] DiagonalDirections = new[]
    {
        Vector2i.UpRight,
        Vector2i.DownRight,
        Vector2i.DownLeft,
        Vector2i.UpLeft,
    };
    
    public static Vector2i[] DefaultDirections = new[]
    {
        Vector2i.Up,
        Vector2i.Right,
        Vector2i.Down,
        Vector2i.Left,
    };
    
    /// <inheritdoc/>
    public override void Initialize()
    {
        Subs.CVar(CfgManager, GameConfigVars.SubGridSize, OnSubGridSizeChanged);
        
        _chunkQuery = GetEntityQuery<SubGridChunkComponent>();
    }

    private void OnSubGridSizeChanged(int num)
    {
        // Values from 0 to 5 are allowed,
        // since it creates a set of numbers that are a power of 2 and bigger than PixelsPerMeter constant.
        if (num < 0 || num > 5)
        {
            Log.Error("SubGridSize must be an integer in the 0-5 range!");
            return;
        }
        
        SubGridSize = num;
    }
    
    
}
