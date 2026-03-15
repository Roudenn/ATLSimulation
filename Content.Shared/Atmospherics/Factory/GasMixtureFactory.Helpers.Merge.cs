using System.Numerics;
using Content.Shared.Atmospherics.GasMixtures;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    /// <summary>
    /// Merges two gas mixtures into one, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge. This will be modified to contain the merged result.</param>
    /// <param name="mB">The second <see cref="IGasMixture"/> to merge.</param>
    /// <remarks>
    /// This variation doesn't modify the volume of the first gas mixture.
    /// If you need that behaviour, use <see cref="MergeWithVolume{T1,T2}(ref T1, ref T2)"/>
    /// </remarks>
    [PublicAPI]
    public void Merge<T1, T2>(ref T1 mA, ref T2 mB)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var combinedHeatCapacity = GetHeatCapacity(ref mA) + GetHeatCapacity(ref mB);
        var temp = (GetInternalEnergy(ref mA) + GetHeatCapacity(ref mB)) / combinedHeatCapacity;
        mA.Temperature = temp;
        NumericsHelpers.Add(mA.Moles, mB.Moles);
    }

    /// <summary>
    /// Merges two gas mixtures into one, conserving total internal energy and mass and combining their volume together.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge. This will be modified to contain the merged result.</param>
    /// <param name="mB">The second <see cref="IGasMixture"/> to merge.</param>
    [PublicAPI]
    public void MergeWithVolume<T1, T2>(ref T1 mA, ref T2 mB)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        Merge(ref mA, ref mB);
        mA.Volume += mB.Volume;
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge.
    /// This will be modified to contain the merged result.</param>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="temp">A temporary <see cref="IGasMixture"/> used to perform the merge.</param>
    [PublicAPI]
    public void Merge<T1, T2, T3>(ref T1 mA, T2[] mN, ref T3 temp)
        where T1 : IGasMixture
        where T2 : IGasMixture
        where T3 : IGasMixture
    {
        // merge the first array and then merge the result with mA to avoid alloc
        Merge(mN, ref temp);
        Merge(ref mA, ref temp);
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="result">The modified <see cref="IGasMixture"/> containing the merged result.</param>
    /// <remarks>
    /// This variation doesn't modify the volume of the resulting gas mixture.
    /// If you need that behaviour, use <see cref="MergeWithVolume{T1,T2}(T1[], ref T2)"/>
    /// </remarks>
    [PublicAPI]
    public void Merge<T1, T2>(T1[] mN, ref T2 result)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var totalHeatCapacity = 0f;
        var totalEnergy = 0f;

        foreach (var m in mN)
        {
            var gasMixture = m; // TODO consider making all of Get() methods not by-ref
            totalHeatCapacity += GetHeatCapacity(ref gasMixture);
            totalEnergy += GetInternalEnergy(ref gasMixture);
            NumericsHelpers.Add(GasBuffer, gasMixture.Moles);
        }

        result.Temperature = totalEnergy / totalHeatCapacity;
        result.Moles = GasBuffer;
        ClearBuffer(ref GasBuffer);
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="result">The modified <see cref="IGasMixture"/> containing the merged result.</param>
    [PublicAPI]
    public void MergeWithVolume<T1, T2>(T1[] mN, ref T2 result)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var totalHeatCapacity = 0f;
        var totalEnergy = 0f;
        var totalVolume = 0f;

        foreach (var m in mN)
        {
            var gasMixture = m; // TODO consider making all of Get() methods not by-ref
            totalHeatCapacity += GetHeatCapacity(ref gasMixture);
            totalEnergy += GetInternalEnergy(ref gasMixture);
            totalVolume += gasMixture.Volume;
            NumericsHelpers.Add(GasBuffer, gasMixture.Moles);
        }

        result.Temperature = totalEnergy / totalHeatCapacity;
        result.Volume = totalVolume;
        result.Moles = GasBuffer;
        ClearBuffer(ref GasBuffer);
    }

        /// <summary>
    /// Merges two gas mixtures into one, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge. This will be modified to contain the merged result.</param>
    /// <param name="mB">The second <see cref="IGasMixture"/> to merge.</param>
    /// <remarks>
    /// This variation doesn't modify the volume of the first gas mixture.
    /// If you need that behaviour, use <see cref="MergeWithVolume{T1,T2}(ref T1, ref T2)"/>
    /// </remarks>
    [PublicAPI]
    public void MergeVelocity(ref VelocityGasMixture mA, ref VelocityGasMixture mB)
    {
        Merge(ref mA, ref mB);
        mA.Velocity += mB.Velocity;
    }

    /// <summary>
    /// Merges two gas mixtures into one, conserving total internal energy and mass and combining their volume together.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge. This will be modified to contain the merged result.</param>
    /// <param name="mB">The second <see cref="IGasMixture"/> to merge.</param>
    [PublicAPI]
    public void MergeVelocityWithVolume(ref VelocityGasMixture mA, ref VelocityGasMixture mB)
    {
        MergeVelocity(ref mA, ref mB);
        mA.Volume += mB.Volume;
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy.
    /// </summary>
    /// <param name="mA">The first <see cref="IGasMixture"/> to merge.
    /// This will be modified to contain the merged result.</param>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="temp">A temporary <see cref="IGasMixture"/> used to perform the merge.</param>
    [PublicAPI]
    public void MergeVelocity(ref VelocityGasMixture mA, VelocityGasMixture[] mN, ref VelocityGasMixture temp)
    {
        // merge the first array and then merge the result with mA to avoid alloc
        MergeVelocity(mN, ref temp);
        MergeVelocity(ref mA, ref temp);
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="result">The modified <see cref="IGasMixture"/> containing the merged result.</param>
    /// <remarks>
    /// This variation doesn't modify the volume of the resulting gas mixture.
    /// If you need that behaviour, use <see cref="MergeVelocityWithVolume(ref VelocityGasMixture, ref VelocityGasMixture)"/>
    /// </remarks>
    [PublicAPI]
    public void MergeVelocity(VelocityGasMixture[] mN, ref VelocityGasMixture result)
    {
        var totalHeatCapacity = 0f;
        var totalEnergy = 0f;
        var totalVelocity = Vector2.Zero;

        foreach (var m in mN)
        {
            var gasMixture = m; // TODO consider making all of Get() methods not by-ref
            totalHeatCapacity += GetHeatCapacity(ref gasMixture);
            totalEnergy += GetInternalEnergy(ref gasMixture);
            totalVelocity += gasMixture.Velocity;
            NumericsHelpers.Add(GasBuffer, gasMixture.Moles);
        }

        result.Temperature = totalEnergy / totalHeatCapacity;
        result.Moles = GasBuffer;
        result.Velocity = totalVelocity;
        ClearBuffer(ref GasBuffer);
    }

    /// <summary>
    /// Merges an array of <see cref="IGasMixture"/>s into a single heat container, conserving total internal energy and mass.
    /// </summary>
    /// <param name="mN">The array of <see cref="IGasMixture"/>s to merge.</param>
    /// <param name="result">The modified <see cref="IGasMixture"/> containing the merged result.</param>
    [PublicAPI]
    public void MergeVelocityWithVolume(VelocityGasMixture[] mN, ref VelocityGasMixture result)
    {
        var totalHeatCapacity = 0f;
        var totalEnergy = 0f;
        var totalVolume = 0f;
        var totalVelocity = Vector2.Zero;

        foreach (var m in mN)
        {
            var gasMixture = m; // TODO consider making all of Get() methods not by-ref
            totalHeatCapacity += GetHeatCapacity(ref gasMixture);
            totalEnergy += GetInternalEnergy(ref gasMixture);
            totalVolume += gasMixture.Volume;
            totalVelocity += gasMixture.Velocity;
            NumericsHelpers.Add(GasBuffer, gasMixture.Moles);
        }

        result.Temperature = totalEnergy / totalHeatCapacity;
        result.Volume = totalVolume;
        result.Moles = GasBuffer;
        result.Velocity = totalVelocity;
        ClearBuffer(ref GasBuffer);
    }
}
