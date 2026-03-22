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
        DiffuseMixturesQuery(ref m1, ref m2, ref GasBuffer3, ref GasBuffer4, cLength, frameTime);
        TransferMoles(ref m1, ref m2, GasBuffer3, GasBuffer4);
        ClearBuffer(ref GasBuffer3, ref GasBuffer4);
    }

    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(
        ref T1 m1,
        ref T2 m2,
        ref float[] diffusion1,
        ref float[] diffusion2,
        float cLength,
        float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixturesQuery(ref m1, ref m2, ref diffusion1, ref diffusion2, cLength, frameTime);
        TransferMoles(ref m1, ref m2, diffusion1, diffusion2);
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
    /// <param name="diffusion1">Array of moles transferred from first mixture to the second.</param>
    /// <param name="diffusion2">Array of moles transferred from second mixture to the first.</param>
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
    public void DiffuseMixturesQuery<T1, T2>(
        ref T1 m1,
        ref T2 m2,
        ref float[] diffusion1,
        ref float[] diffusion2,
        float cLength,
        float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtureQuery(ref m1, ref m2, ref diffusion1, cLength, frameTime);
        DiffuseMixtureQuery(ref m2, ref m1, ref diffusion2, cLength, frameTime);
    }

    /// <summary>
    /// Calculates the amounts of moles that would be diffused between two <see cref="IGasMixture"/>s,
    /// given some characteristic length between two bodies and a small time delta.
    /// This variation assumes that the two gas mixtures are rectangular prisms of the same size,
    /// where characteristic length is the height of both mixtures.
    /// </summary>
    /// <param name="m1">First gas mixture to diffuse to.</param>
    /// <param name="m2">Second gas mixture to diffuse from.</param>
    /// <param name="diffusion">Array of moles transferred from first mixture to the second.</param>
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
    public void DiffuseMixtureQuery<T1, T2>(ref T1 m1, ref T2 m2, ref float[] diffusion, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        // Find the difference in partial pressure of each gas.
        var temperature = (m1.Temperature + m2.Temperature) / 2f;
        GetPartialPressures(m1.Moles, temperature, m1.Volume, ref GasBufferResults1);
        GetPartialPressures(m2.Moles, temperature, m2.Volume, ref GasBufferResults2);
        NumericsHelpers.Sub(GasBufferResults1, GasBufferResults2, GasBufferResults1);
        NumericsHelpers.Max(GasBufferResults1, 0f);

        // Calculate the partial result
        NumericsHelpers.Multiply(GasBufferResults1, m1.Volume * cLength * frameTime / MathF.Sqrt(PhysicalConstants.R * m1.Temperature) * 1000f);

        // Multiply moles by their beta sizes
        NumericsHelpers.Multiply(GasAtomBetaSizes, m1.Moles, GasBuffer1);
        NumericsHelpers.Max(GasBuffer1, SystemConstants.Epsilon); // Prevent division by zero

        // Get the results
        NumericsHelpers.Divide(GasBufferResults1, GasBuffer1, diffusion);
        ClearBuffer(ref GasBuffer1, ref GasBufferResults1, ref GasBuffer2);
    }
}
