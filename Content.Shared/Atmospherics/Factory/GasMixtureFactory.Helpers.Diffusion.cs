using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(ref T1 m1, ref T2 m2, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixturesQuery(ref m1, ref m2, ref GasBuffer3, cLength, frameTime);
        TransferMoles(ref m1, ref m2, GasBuffer3);
        ClearBuffer(ref GasBuffer3);
    }

    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(ref T1 m1, ref T2 m2, ref float[] diffusions, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixturesQuery(ref m1, ref m2, ref diffusions, cLength, frameTime);
        TransferMoles(ref m1, ref m2, diffusions);
    }

    // TODO this right now assumes that two mixtures have the same size,
    // an overload that supports any surface areas and characteristic lengths is required.
    /// <summary>
    /// Calculates the amounts of moles that would be diffused between two <see cref="IGasMixture"/>s,
    /// given some characteristic length between two bodies and a small time delta.
    /// This variation assumes that the two gas mixtures are rectangular prisms of the same size,
    /// where characteristic length is the height of both mixtures.
    /// </summary>
    /// <param name="m1">First gas mixture to diffuse to.</param>
    /// <param name="m2">Second gas mixture to diffuse from.</param>
    /// <param name="diffusions">Array that stores the results.</param>
    /// <param name="cLength">Characteristic length of both gas mixtures, or their shared "height".</param>
    /// <param name="frameTime">The amount of time that the moles are allowed to be transferred, in seconds.</param>
    /// <remarks>
    /// This solves the first Fick's law for each gas in both mixtures.
    ///
    /// The simplified formula looks like this:
    /// ∆γ = (V h t ∆p) / (β γ √RT)
    /// Where coefficient Beta is precalculated for each gas on initialize:
    /// β = 1.5 π^(3/2) d^2 N_A M^(1/2)
    ///
    /// This is derived by:
    /// 1. Converting diffusion flow into amount of moles ∆γ=D*(St∆p)/(RTL)
    /// 2. Calculating diffusion coefficient for a perfect gas: D ≈ 1/3 λ v_avg
    /// 2.1. Calculating mean free path: λ = V / (√2 πd^2 γN_A)
    /// 2.2. Calculating average speed: v_avg = √(8RT/πM)
    /// 3. When combine it looks like this: ∆γ=1/3∙V/(√2 βγ)∙√(8RT/πM)∙St∆P/RTL
    /// 4. And then some elements cancel out, leaving this formula: ∆γ_i=2/3∙(VHt∆p_i)/(π^(3/2) d^2 N_A γ_i √(M_i RT))
    /// </remarks>
    [PublicAPI]
    public void DiffuseMixturesQuery<T1, T2>(ref T1 m1, ref T2 m2, ref float[] diffusions, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        // Calculate the parts that are the same for all gases.
        var sharedCoefficient = m1.Volume * cLength * frameTime / MathF.Sqrt(PhysicalConstants.R * m1.Temperature) * 1000f;

        // Top part for the first diffusion transfer
        GetPartialPressures(ref m1, ref GasBufferResults1);
        NumericsHelpers.Multiply(GasBufferResults1, sharedCoefficient);
        // Bottom part for the first diffusion transfer
        NumericsHelpers.Multiply(GasAtomBetaSizes, m1.Moles, GasBuffer1);
        NumericsHelpers.Multiply(GasBuffer1, 0.001f); // Convert g/moles to kg/moles
        NumericsHelpers.Max(GasBuffer1, SystemConstants.MinimumHeatCapacity);
        // Results for the first diffusion transfer
        NumericsHelpers.Divide(GasBufferResults1, GasBuffer1);

        // Top part for the second diffusion transfer
        GetPartialPressures(ref m2, ref GasBufferResults2);
        NumericsHelpers.Multiply(GasBufferResults2, sharedCoefficient);
        // Bottom part for the second diffusion transfer
        NumericsHelpers.Multiply(GasAtomBetaSizes, m2.Moles, GasBuffer2);
        NumericsHelpers.Multiply(GasBuffer2, 0.001f); // Convert g/moles to kg/moles
        NumericsHelpers.Max(GasBuffer2, SystemConstants.MinimumHeatCapacity);
        // Results for the second diffusion transfer
        NumericsHelpers.Divide(GasBufferResults2, GasBuffer2);

        // Combine them into a single array
        NumericsHelpers.Sub(GasBufferResults1, GasBufferResults2, diffusions);
        ClearBuffer(ref GasBuffer1, ref GasBuffer2);
        ClearBuffer(ref GasBufferResults1, ref GasBufferResults2);
    }
}
