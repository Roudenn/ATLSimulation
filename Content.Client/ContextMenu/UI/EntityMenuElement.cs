using Content.Client.UserInterface;

namespace Content.Client.ContextMenu.UI
{
    public sealed class EntityMenuElement : ContextMenuElement, IEntityControl
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        /// <summary>
        ///     The entity that can be accessed by interacting with this element.
        /// </summary>
        public EntityUid? Entity;

        /// <summary>
        ///     How many entities are accessible through this element's sub-menus.
        /// </summary>
        public int Count { get; private set; }

        public EntityMenuElement(EntityUid? entity = null)
        {
            IoCManager.InjectDependencies(this);

            Entity = entity;
            if (Entity == null)
                return;

            Count = 1;
            UpdateEntity();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Entity = null;
            Count = 0;
        }

        /// <summary>
        ///     Update the entity count
        /// </summary>
        public void UpdateCount()
        {
            if (SubMenu == null)
                return;

            Count = 0;
            foreach (var subElement in SubMenu.MenuBody.Children)
            {
                if (subElement is EntityMenuElement entityElement)
                    Count += entityElement.Count;
            }

            IconLabel.Visible = Count > 1;
            if (IconLabel.Visible)
                IconLabel.Text = Count.ToString();
        }

        private string GetEntityDescriptionAdmin(EntityUid entity)
        {
            var representation = _entityManager.ToPrettyString(entity);

            var name = representation.Name;
            var prototype = representation.Prototype;
            var playerName = representation.Session?.Name;
            var deleted = representation.Deleted;

            return $"{name} ({_entityManager.GetNetEntity(entity).ToString()}{(prototype != null ? $", {prototype}" : "")}{(playerName != null ? $", {playerName}" : "")}){(deleted ? "D" : "")}";
        }

        private string GetEntityDescription(EntityUid entity)
        {
            return GetEntityDescriptionAdmin(entity);
        }

        /// <summary>
        ///     Update the icon and text of this element based on the given entity or this element's own entity if none
        ///     is provided.
        /// </summary>
        public void UpdateEntity(EntityUid? entity = null)
        {
            entity ??= Entity;

            // check whether entity is null, invalid, or has been deleted.
            // _entityManager.Deleted() implicitly checks all of these.
            if (_entityManager.Deleted(entity))
            {
                Icon.SetEntity(null);
                Text = string.Empty;
            }
            else
            {
                Icon.SetEntity(entity);
                Text = GetEntityDescription(entity.Value);
            }
        }

        EntityUid? IEntityControl.UiEntity => Entity;
    }
}
