using Content.Shared.Constants;
using Content.Shared.GameCVars;

namespace Content.Shared.Subgrid.Systems;

public abstract partial class SharedSubGridSystem
{
    /// <summary>
    /// Amount of subdivisions of a single grid tile.
    /// </summary>
    [ViewVariables]
    public int SubGridDivisions { get; private set; }

    /// <summary>
    /// Height of a subgrid tile.
    /// </summary>
    [ViewVariables]
    public float SubGridHeight { get; private set; }

    /// <summary>
    /// Size of a single dimension of a subgrid box inside a single tile.
    /// </summary>
    [ViewVariables]
    public int SubGridTileSize { get; private set; }

    /// <summary>
    /// Real size of a single subgrid tile in world units (meters).
    /// </summary>
    [ViewVariables]
    public float SubGridWorldSize { get; private set; } = 1f;

    /// <summary>
    /// Volume of a single subgrid tile.
    /// </summary>
    [ViewVariables]
    public float SubGridTileVolume { get; private set; }

    /// <summary>
    /// Size of a single dimension of a subgrid chunk's box.
    /// </summary>
    [ViewVariables]
    public int SubGridChunkSize { get; private set; }

    /// <summary>
    /// Total amount of tiles in a single subgrid chunk.
    /// </summary>
    [ViewVariables]
    public int SubGridChunkArea { get; private set; }

    private void InitializeCVars()
    {
        Subs.CVar(CfgManager, GameConfigVars.SubGridHeight, OnHeightChanged, true);
        Subs.CVar(CfgManager, GameConfigVars.SubGridSize, OnSubGridSizeChanged, true);
    }

    private void OnHeightChanged(float f)
    {
        SubGridHeight = f;
        UpdateVolumeValue();

        var ev = new SubGridHeightChangedEvent();
        RaiseLocalEvent(ref ev);
    }

    private void OnSubGridSizeChanged(int num)
    {
        // Values from 0 to 5 are allowed,
        // since it creates a set of numbers that are a power of 2 and bigger than PixelsPerMeter constant.
        if (num < 0 || num > 5)
            num = Math.Clamp(num, 0, 5);

        SubGridDivisions = num;
        SubGridTileSize = 1 << num; // Equivalent to Math.Pow(2, num)
        SubGridWorldSize = 1 / (float) (1 << num);
        SubGridChunkSize = SystemConstants.PvsChunkSize * (1 << num);
        SubGridChunkArea = SystemConstants.PvsChunkSize * (1 << num) * SystemConstants.PvsChunkSize * (1 << num);
        UpdateVolumeValue();

        var ev = new SubGridResizedEvent();
        RaiseLocalEvent(ref ev);
    }

    private void UpdateVolumeValue()
    {
        SubGridTileVolume = SubGridWorldSize * SubGridWorldSize * SubGridHeight;
    }
}
