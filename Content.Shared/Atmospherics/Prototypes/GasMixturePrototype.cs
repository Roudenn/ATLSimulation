using Content.Shared.Atmospherics.GasMixtures;
using Content.Shared.Atmospherics.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared.Atmospherics.Prototypes;

/// <summary>
/// A prototype for gas mixture composition and temperature.
/// Used by <see cref="SharedAtmosphericsSystem"/> in order to create <see cref="GasMixture"/> instances.
/// </summary>
[Prototype]
public sealed partial class GasMixturePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MixtureDefinition Definition = new MixtureCompositionDefinition();
}
