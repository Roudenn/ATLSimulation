using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Constants;
using Content.Shared.Utils;
using JetBrains.Annotations;

namespace Content.Shared.Atmospherics.Factory;

public sealed partial class GasMixtureFactory
{
    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(ref T1 m1, ref T2 m2, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtures(ref m1, ref m2, cLength, frameTime, SharedPool);
    }

    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(ref T1 m1, ref T2 m2, float cLength, float frameTime, IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        DiffuseMixturesQuery(ref m1, ref m2, buffer1, buffer2, cLength, frameTime);
        TransferMoles(ref m1, ref m2, buffer1, buffer2);
        pool.Return(buffer1);
        pool.Return(buffer2);
    }

    [PublicAPI]
    public void DiffuseTiles(ref TileAtmos t1, TileAtmos t2, float cLength, float frameTime)
    {
        DiffuseTiles(ref t1, t2, cLength, frameTime, SharedPool);
    }

    [PublicAPI]
    public void DiffuseTiles(ref TileAtmos t1, TileAtmos t2, float cLength, float frameTime, IRobustArrayPool<float> pool)
    {
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        DiffuseMixturesQuery(ref t1.Mixture, ref t2.Mixture, buffer1, buffer2, cLength, frameTime, pool);
        TransferMoles(ref t1.CachedMixture, ref t2.CachedMixture, buffer1, buffer2, pool);
        pool.Return(buffer1);
        pool.Return(buffer2);
    }

    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(
        ref T1 m1,
        ref T2 m2,
        Span<float> diffusion1,
        Span<float> diffusion2,
        float cLength,
        float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtures(ref m1, ref m2, diffusion1, diffusion2, cLength, frameTime, SharedPool);
    }

    [PublicAPI]
    public void DiffuseMixtures<T1, T2>(
        ref T1 m1,
        ref T2 m2,
        Span<float> diffusion1,
        Span<float> diffusion2,
        float cLength,
        float frameTime,
        IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixturesQuery(ref m1, ref m2, diffusion1, diffusion2, cLength, frameTime, pool);
        TransferMoles(ref m1, ref m2, diffusion1, diffusion2, pool);
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
        Span<float> diffusion1,
        Span<float> diffusion2,
        float cLength,
        float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtureQuery(ref m1, ref m2, diffusion1, cLength, frameTime);
        DiffuseMixtureQuery(ref m2, ref m1, diffusion2, cLength, frameTime);
    }

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
    /// <param name="pool">Pool for taking gas buffers and using them for calculations.</param>
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
        Span<float> diffusion1,
        Span<float> diffusion2,
        float cLength,
        float frameTime,
        IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtureQuery(ref m1, ref m2, diffusion1, cLength, frameTime, pool);
        DiffuseMixtureQuery(ref m2, ref m1, diffusion2, cLength, frameTime, pool);
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
    public void DiffuseMixtureQuery<T1, T2>(ref T1 m1, ref T2 m2, Span<float> diffusion, float cLength, float frameTime)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        DiffuseMixtureQuery(ref m1, ref m2, diffusion, cLength, frameTime, SharedPool);
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
    /// <param name="pool">Pool for taking gas buffers and using them for calculations.</param>
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
    public void DiffuseMixtureQuery<T1, T2>(ref T1 m1, ref T2 m2, Span<float> diffusion, float cLength, float frameTime, IRobustArrayPool<float> pool)
        where T1 : IGasMixture
        where T2 : IGasMixture
    {
        var buffer1 = pool.Rent();
        var buffer2 = pool.Rent();
        var buffer3 = pool.Rent();

        // Find the difference in concentration of each gas.
        GetPartialPressures(m1.Moles, m1.Temperature, m1.Volume, buffer1);
        NumericsHelpers.Divide(buffer1, PhysicalConstants.R * m1.Temperature * 0.001f);
        GetPartialPressures(m2.Moles, m1.Temperature, m2.Volume, buffer2);
        NumericsHelpers.Divide(buffer2, PhysicalConstants.R * m2.Temperature * 0.001f);

        NumericsHelpers.Sub(buffer1, buffer2);
        NumericsHelpers.Max(buffer1, 0f); // TODO Because of "Prevent division by zero" we have to calculate the thing twice

        // Calculate the partial result
        NumericsHelpers.Multiply(buffer1,
            m1.Volume * cLength * frameTime * MathF.Sqrt(PhysicalConstants.R * ((m1.Temperature + m2.Temperature) / 2f)));

        // Multiply moles by their beta sizes
        NumericsHelpers.Multiply(Prototypes.GasAtomBetaSizes, m1.Moles, buffer3);
        NumericsHelpers.Max(buffer3, SystemConstants.Epsilon); // Prevent division by zero

        // Get the results
        NumericsHelpers.Divide(buffer1, buffer3, diffusion);

        pool.Return(buffer1);
        pool.Return(buffer2);
        pool.Return(buffer3);
    }
}
