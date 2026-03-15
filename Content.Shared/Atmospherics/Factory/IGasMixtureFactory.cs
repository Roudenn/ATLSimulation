using System.Diagnostics.CodeAnalysis;

namespace Content.Shared.Atmospherics.Factory;

public interface IGasMixtureFactory : IEnumerable<GasPrototype>
{
        /// <summary>
        ///     Indexer to retrieve a gas definition by name.
        /// </summary>
        /// <param name="name">The name of the gas definition.</param>
        /// <returns>The named gas definition.</returns>
        GasPrototype this[string name] { get; }

        /// <summary>
        ///     Indexer to retrieve a gas definition by internal ID.
        /// </summary>
        /// <param name="id">The ID of the gas definition.</param>
        /// <returns>The gas definition.</returns>
        GasPrototype this[int id] { get; }

        /// <summary>
        /// Try to retrieve a gas definition by name.
        /// </summary>
        /// <param name="name">The name of the gas definition to look up.</param>
        /// <param name="definition">The found gas definition, if it exists.</param>
        /// <returns>True if a gas definition was resolved, false otherwise.</returns>
        /// <seealso cref="this[string]"/>
        bool TryGetDefinition(string name, [NotNullWhen(true)] out GasPrototype? definition);

        /// <summary>
        /// Try to retrieve a gas definition by gas ID.
        /// </summary>
        /// <param name="id">The ID of the gas definition to look up.</param>
        /// <param name="definition">The found gas definition, if it exists.</param>
        /// <returns>True if a gas definition was resolved, false otherwise.</returns>
        /// <seealso cref="this[int]"/>
        bool TryGetDefinition(int id, [NotNullWhen(true)] out GasPrototype? definition);

        /// <summary>
        ///     The number of gas definitions contained inside of this manager.
        /// </summary>
        int Count { get; }

        /// <summary>
        ///     Array size that should be applied to all gas mixtures.
        /// </summary>
        int ArraySize { get; }

        void Initialize();

        /// <summary>
        ///     Register a gas definition with this manager.
        /// </summary>
        /// <param name="gasDef">THe definition to register.</param>
        void Register(GasPrototype gasDef);
}
