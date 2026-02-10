namespace Content.Shared.Constants;

public static class SystemConstants
{
    /// <summary>
    /// A size of the PvsChunk, rounded closer towards the bigger value.
    /// This is duplicated from the server-side PvsSystem so client can also access this value.
    /// Required for delicious memory performance.
    /// </summary>
    public const int PvsChunkSize = 8;
    
    /// <summary>
    /// The amount of pixels per meter in the simulation.
    /// </summary>
    public const int PixelsPerMeter = 32;
    
    /// <summary>
    ///     Do not allow any gas mixture temperatures to exceed this number. It is occasionally possible
    ///     to have very small heat capacity (e.g. room that was just unspaced) and for large amounts of
    ///     energy to be transferred to it, even for a brief moment. However, this messes up subsequent
    ///     calculations and so cap it here. The physical interpretation is that at this temperature, any
    ///     gas that you would have transforms into plasma.
    /// </summary>
    public const float Tmax = 262144; // 1/64 of max safe integer, any values above will result in a ~0.03K epsilon
    
    /// <summary>
    ///     Minimum number of moles a gas can have.
    /// </summary>
    public const float GasMinMoles = 0.00000005f;
    
    /// <summary>
    ///     Minimum heat capacity.
    /// </summary>
    public const float MinimumHeatCapacity = 0.0003f;
}
