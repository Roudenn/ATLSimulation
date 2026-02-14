using Content.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Examine
{
    public abstract partial class ExamineSystemShared : EntitySystem
    {
        public const string DefaultIconTexture = "/Textures/Interface/examine-star.png";

        public override void Initialize()
        {
            base.Initialize();

            _ghostQuery = GetEntityQuery<ObserverComponent>();
        }

        /// <summary>
        ///     Checks if the entity <paramref name="uid"/> has any of the listed <paramref name="components"/>.
        /// </summary>
        public bool EntityHasComponent(EntityUid uid, List<string> components)
        {
            foreach (var comp in components)
            {
                if (!Factory.TryGetRegistration(comp, out var componentRegistration))
                    continue;

                if (!HasComp(uid, componentRegistration.Type))
                    continue;

                return true;
            }
            return false;
        }

        /// <summary>
        ///     Sends an ExamineTooltip based on the contents of <paramref name="group"/>
        /// </summary>
        public void SendExamineGroup(EntityUid user, EntityUid target, ExamineGroup group)
        {
            var message = new FormattedMessage();

            if (group.Title != null)
            {
                message.AddMarkupOrThrow(Loc.GetString(group.Title));
                message.PushNewline();
            }
            message.AddMessage(GetFormattedMessageFromExamineEntries(group.Entries));

            SendExamineTooltip(user, target, message, getVerbs: false, centerAtCursor: false);
        }

        /// <returns>A FormattedMessage based on all <paramref name="entries"/>, sorted.</returns>
        public static FormattedMessage GetFormattedMessageFromExamineEntries(List<ExamineEntry> entries)
        {
            var formattedMessage = new FormattedMessage();
            entries.Sort((a, b) => (b.Priority.CompareTo(a.Priority)));

            var first = true;

            foreach (var entry in entries)
            {
                if (!first)
                {
                    formattedMessage.PushNewline();
                }
                else
                {
                    first = false;
                }

                formattedMessage.AddMessage(entry.Message);
            }

            return formattedMessage;
        }
    }
}
