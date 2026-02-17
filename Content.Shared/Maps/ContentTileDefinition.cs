using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

namespace Content.Shared.Maps
{
    [Prototype("tile")]
    public sealed partial class ContentTileDefinition : IInheritingPrototype, ITileDefinition
    {
        public const string SpaceID = "Space";

        [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<ContentTileDefinition>))]
        public string[]? Parents { get; private set; }

        [NeverPushInheritance]
        [AbstractDataField]
        public bool Abstract { get; private set; }

        [IdDataField]
        public string ID { get; private set; } = string.Empty;

        public ushort TileId { get; private set; }

        [DataField]
        public string Name { get; private set; } = "";

        [DataField]
        public ResPath? Sprite { get; private set; }

        [DataField]
        public Dictionary<Direction, ResPath> EdgeSprites { get; private set; } = new();

        [DataField]
        public int EdgeSpritePriority { get; private set; }

        /// <summary>
        /// Base friction modifier for this tile.
        /// </summary>
        [DataField]
        public float Friction { get; set; } = 1f;

        [DataField]
        public byte Variants { get; set; } = 1;

        [DataField]
        public bool MapAtmosphere { get; private set; }

        public void AssignTileId(ushort id)
        {
            TileId = id;
        }
    }
}
