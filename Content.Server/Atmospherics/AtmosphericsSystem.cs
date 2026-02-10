using Content.Server.SubGrid;
using Content.Shared.Atmospherics;
using Content.Shared.Atmospherics.Systems;
using Content.Shared.Subgrid.Components;
using Content.Shared.Subgrid.Systems;
using Content.Shared.Temperature;

namespace Content.Server.Atmospherics;

public sealed partial class AtmosphericsSystem : SharedAtmosphericsSystem
{
    [Dependency] private readonly SubGridSystem _subGrid = default!;
    
    public bool AtmosEnabled;
    public bool AtmosEightDirections;
    
    /// <inheritdoc/>
    public override void Initialize()
    {
        
    }

    /// <summary>
    /// Updates a whole grid
    /// </summary>
    /// <param name="chunks"></param>
    /// <param name="frameTime"></param>
    public void ProcessAtmosGrid(ref Dictionary<Vector2i, TileAtmosphere[]> chunks, float frameTime)
    {
        foreach (var (chunkPos, chunk) in chunks)
        {
            for (int i = 0; i < chunk.Length; i++)
            {
                ProcessAtmosTile(ref chunks, chunkPos, i, frameTime);
            }
        }
    }

    /// <summary>
    /// Updates an atmos tile 
    /// </summary>
    /// <param name="chunks"></param>
    /// <param name="chunkPos"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private void ProcessAtmosTile(ref Dictionary<Vector2i, TileAtmosphere[]> chunks, Vector2i chunkPos, int index, float frameTime)
    {
        foreach (var dir in SharedSubGridSystem.DefaultDirections)
        {
            if (!_subGrid.TryGetAtmosphereTileRelative(chunks, chunkPos, index, dir, out var neighbour))
            {
                Log.Error($"");
                continue;
            }
            
            // process the stuff here later
        }
        
        foreach (var dir in SharedSubGridSystem.DiagonalDirections)
        {
            if (!_subGrid.TryGetAtmosphereTileRelative(chunks, chunkPos, index, dir, out var neighbour))
            {
                Log.Error($"");
                continue;
            }
            
            // process the stuff here later
        }
        
        
        
        
    }
}