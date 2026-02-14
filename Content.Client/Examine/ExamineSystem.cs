using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Threading;
using Content.Client.Gameplay;
using Content.Shared.Examine;
using Content.Shared.GameCVars;
using Content.Shared.Input;
using Content.Shared.Tag;
using JetBrains.Annotations;
using Robust.Client.ComponentTrees;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Configuration;
using Robust.Shared.Containers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using static Robust.Client.UserInterface.Controls.BoxContainer;
using Direction = Robust.Shared.Maths.Direction;

namespace Content.Client.Examine
{
    [UsedImplicitly]
    public sealed class ExamineSystem : ExamineSystemShared
    {
        [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
        [Dependency] private readonly IPlayerManager _playerManager = default!;
        [Dependency] private readonly IEyeManager _eyeManager = default!;
        [Dependency] private readonly IStateManager _stateManager = default!;
        [Dependency] private readonly SpriteTreeSystem _tree = default!;
        [Dependency] private readonly TagSystem _tagSystem = default!;
        [Dependency] private readonly SharedContainerSystem _containers = default!;
        [Dependency] private readonly IConfigurationManager _cfg = default!;
        [Dependency] private readonly EntityLookupSystem _lookup = default!;

        private float _lookupSize;

        private static readonly ProtoId<TagPrototype> HideContextMenuTag = "HideContextMenu";

        public const string StyleClassEntityTooltip = "entity-tooltip";

        private EntityUid _examinedEntity;
        private Popup? _examineTooltipOpen;
        private ScreenCoordinates _popupPos;
        private CancellationTokenSource? _requestCancelTokenSource;
        private int _idCounter;

        public override void Initialize()
        {
            base.Initialize();

            UpdatesOutsidePrediction = true;

            SubscribeNetworkEvent<ExamineSystemMessages.ExamineInfoResponseMessage>(OnExamineInfoResponse);

            CommandBinds.Builder
                .Bind(ContentKeyFunctions.ExamineEntity, new PointerInputCmdHandler(HandleExamine, outsidePrediction: true))
                .Register<ExamineSystem>();

            _idCounter = 0;

            Subs.CVar(_cfg, GameConfigVars.GameEntityMenuLookup, OnLookupChanged, true);
        }

        private void OnLookupChanged(float val)
        {
            _lookupSize = val;
        }

        public override void Update(float frameTime)
        {
            if (_examineTooltipOpen is not {Visible: true}) return;
            if (!_examinedEntity.Valid || _playerManager.LocalEntity is not { } player) return;

            if (!CanExamine(player, _examinedEntity))
                CloseTooltip();
        }

        public override void Shutdown()
        {
            CommandBinds.Unregister<ExamineSystem>();
            base.Shutdown();
        }

        public override bool CanExamine(EntityUid examiner, MapCoordinates target, Ignored? predicate = null, EntityUid? examined = null, ExaminerComponent? examinerComp = null)
        {
            if (!Resolve(examiner, ref examinerComp, false))
                return false;

            if (examinerComp.SkipChecks)
                return true;

            if (examinerComp.CheckInRangeUnOccluded)
            {
                // TODO fix this. This should be using the examiner's eye component, not eye manager.
                var b = _eyeManager.GetWorldViewbounds();
                if (!b.Contains(target.Position))
                    return false;
            }

            return base.CanExamine(examiner, target, predicate, examined, examinerComp);
        }

        private bool HandleExamine(in PointerInputCmdHandler.PointerInputCmdArgs args)
        {
            var entity = args.EntityUid;

            if (!args.EntityUid.IsValid() || !Exists(entity))
            {
                return false;
            }

            if (_playerManager.LocalEntity is not { } player ||
                !CanExamine(player, entity))
            {
                return false;
            }

            DoExamine(entity);
            return true;
        }

        private void OnExamineInfoResponse(ExamineSystemMessages.ExamineInfoResponseMessage ev)
        {
            var player = _playerManager.LocalEntity;
            if (player == null)
                return;

            // Prevent updating a new tooltip.
            if (ev.Id != 0 && ev.Id != _idCounter)
                return;

            // Tooltips coming in from the server generally prioritize
            // opening at the old tooltip rather than the cursor/another entity,
            // since there's probably one open already if it's coming in from the server.
            var entity = GetEntity(ev.EntityUid);

            OpenTooltip(player.Value, entity, ev.CenterAtCursor, ev.OpenAtOldTooltip, ev.KnowTarget);
            UpdateTooltipInfo(player.Value, entity, ev.Message, getVerbs: false);
        }

        public override void SendExamineTooltip(EntityUid player, EntityUid target, FormattedMessage message, bool getVerbs, bool centerAtCursor)
        {
            OpenTooltip(player, target, centerAtCursor);
            UpdateTooltipInfo(player, target, message, getVerbs: getVerbs);
        }

        /// <summary>
        ///     Opens the tooltip window and sets spriteview/name/etc, but does
        ///     not fill it with information. This is done when the server sends examine info/verbs,
        ///     or immediately if it's entirely clientside.
        /// </summary>
        public void OpenTooltip(EntityUid player, EntityUid target, bool centeredOnCursor=true, bool openAtOldTooltip=true, bool knowTarget = true)
        {
            // Close any examine tooltip that might already be opened
            // Before we do that, save its position. We'll prioritize opening any new popups there if
            // openAtOldTooltip is true.
            ScreenCoordinates? oldTooltipPos = _examineTooltipOpen != null ? _popupPos : null;
            CloseTooltip();

            // cache entity for Update function
            _examinedEntity = target;

            const float minWidth = 300;

            if (openAtOldTooltip && oldTooltipPos != null)
            {
                _popupPos = oldTooltipPos.Value;
            }
            else if (centeredOnCursor)
            {
                _popupPos = _userInterfaceManager.MousePositionScaled;
            }
            else
            {
                _popupPos = _eyeManager.CoordinatesToScreen(Transform(target).Coordinates);
                _popupPos = _userInterfaceManager.ScreenToUIPosition(_popupPos);
            }

            // Actually open the tooltip.
            _examineTooltipOpen = new Popup { MaxWidth = 400 };
            _userInterfaceManager.ModalRoot.AddChild(_examineTooltipOpen);
            var panel = new PanelContainer() { Name = "ExaminePopupPanel" };
            panel.AddStyleClass(StyleClassEntityTooltip);
            panel.ModulateSelfOverride = Color.LightGray.WithAlpha(0.90f);
            _examineTooltipOpen.AddChild(panel);

            var vBox = new BoxContainer
            {
                Name = "ExaminePopupVbox",
                Orientation = LayoutOrientation.Vertical,
                MaxWidth = _examineTooltipOpen.MaxWidth
            };
            panel.AddChild(vBox);

            var hBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                SeparationOverride = 5,
                Margin = new Thickness(6, 0, 6, 0)
            };

            vBox.AddChild(hBox);

            if (HasComp<SpriteComponent>(target))
            {
                var spriteView = new SpriteView
                {
                    OverrideDirection = Direction.South,
                    SetSize = new Vector2(32, 32)
                };
                spriteView.SetEntity(target);
                hBox.AddChild(spriteView);
            }

            if (knowTarget)
            {
                var itemName = FormattedMessage.EscapeText(EntityManager.GetComponent<MetaDataComponent>(target).EntityName);
                var labelMessage = FormattedMessage.FromMarkupPermissive($"[bold]{itemName}[/bold]");
                var label = new RichTextLabel();
                label.SetMessage(labelMessage);
                hBox.AddChild(label);
            }
            else
            {
                var label = new RichTextLabel();
                label.SetMessage(FormattedMessage.FromMarkupOrThrow("[bold]???[/bold]"));
                hBox.AddChild(label);
            }

            panel.Measure(Vector2Helpers.Infinity);
            var size = Vector2.Max(new Vector2(minWidth, 0), panel.DesiredSize);

            _examineTooltipOpen.Open(UIBox2.FromDimensions(_popupPos.Position, size));
        }

        /// <summary>
        ///     Fills the examine tooltip with a message and buttons if applicable.
        /// </summary>
        public void UpdateTooltipInfo(EntityUid player, EntityUid target, FormattedMessage message, bool getVerbs = true)
        {
            var vBox = _examineTooltipOpen?.GetChild(0).GetChild(0);
            if (vBox == null)
            {
                return;
            }

            foreach (var msg in message.Nodes)
            {
                if (msg.Name != null)
                    continue;

                var text = msg.Value.StringValue ?? "";

                if (string.IsNullOrWhiteSpace(text))
                    continue;

                var richLabel = new RichTextLabel() { Margin = new Thickness(4, 4, 0, 4)};
                richLabel.SetMessage(message);
                vBox.AddChild(richLabel);
                break;
            }
        }

        public void DoExamine(EntityUid entity, bool centeredOnCursor = true, EntityUid? userOverride = null)
        {
            var playerEnt = userOverride ?? _playerManager.LocalEntity;
            if (playerEnt == null)
                return;

            FormattedMessage message;

            OpenTooltip(playerEnt.Value, entity, centeredOnCursor, false);

            // Always update tooltip info from client first.
            // If we get it wrong, server will correct us later anyway.
            // This will usually be correct (barring server-only components, which generally only adds, not replaces text)
            message = GetExamineText(entity, playerEnt);
            UpdateTooltipInfo(playerEnt.Value, entity, message);

            if (!IsClientSide(entity))
            {
                // Ask server for extra examine info.
                unchecked
                {
                    _idCounter += 1;
                }
                RaiseNetworkEvent(new ExamineSystemMessages.RequestExamineInfoMessage(GetNetEntity(entity), _idCounter, true));
            }

            RaiseLocalEvent(entity, new ClientExaminedEvent(entity, playerEnt.Value));
        }

        private void CloseTooltip()
        {
            if (_examineTooltipOpen != null)
            {
                _examineTooltipOpen.Dispose();
                _examineTooltipOpen = null;
            }

            if (_requestCancelTokenSource != null)
            {
                _requestCancelTokenSource.Cancel();
                _requestCancelTokenSource = null;
            }
        }

        /// <summary>
        /// Get all of the entities in an area for displaying on the context menu.
        /// </summary>
        /// <returns>True if any entities were found.</returns>
        public bool TryGetEntityMenuEntities(MapCoordinates targetPos, [NotNullWhen(true)] out List<EntityUid>? entities)
        {
            entities = null;

            if (_stateManager.CurrentState is not GameplayStateBase)
                return false;

            if (_playerManager.LocalEntity is not { } player)
                return false;

            // Initially, we include all entities returned by a sprite area lookup
            var box = Box2.CenteredAround(targetPos.Position, new Vector2(_lookupSize, _lookupSize));
            var queryResult = _tree.QueryAabb(targetPos.MapId, box);
            entities = new List<EntityUid>(queryResult.Count);
            foreach (var ent in queryResult)
            {
                entities.Add(ent.Uid);
            }

            // If we're in a container list all other entities in it.
            // E.g., allow players in lockers to examine / interact with other entities in the same locker
            if (_containers.TryGetContainingContainer((player, null), out var container))
            {
                // Only include the container contents when clicking near it.
                if (entities.Contains(container.Owner)
                    || _containers.TryGetOuterContainer(container.Owner, Transform(container.Owner), out var outer)
                    && entities.Contains(outer.Owner))
                {
                    // The container itself might be in some other container, so it might not have been added by the
                    // sprite tree lookup.
                    if (!entities.Contains(container.Owner))
                        entities.Add(container.Owner);

                    // TODO Context Menu
                    // This might miss entities in some situations. E.g., one of the contained entities entity in it, that
                    // itself has another entity attached to it, then we should be able to "see" that entity.
                    // E.g., if a security guard is on a segway and gets thrown in a locker, this wouldn't let you see the guard.
                    foreach (var ent in container.ContainedEntities)
                    {
                        if (!entities.Contains(ent))
                            entities.Add(ent);
                    }
                }
            }

                // This is inefficient, but I'm lazy and CBF implementing my own recursive container method. Note that
                // this might actually fail to add the contained children of some entities in the menu. E.g., an entity
                // with a large sprite aabb, but small broadphase might appear in the menu, but have its children added
                // by this.
                var flags = LookupFlags.All & ~LookupFlags.Sensors;
                foreach (var e in _lookup.GetEntitiesInRange(targetPos, _lookupSize, flags: flags))
                {
                    if (!entities.Contains(e))
                        entities.Add(e);
                }


            for (var i = entities.Count - 1; i >= 0; i--)
            {
                if (_tagSystem.HasTag(entities[i], HideContextMenuTag))
                    entities.RemoveSwap(i);
            }

            // Unless we added entities in containers, every entity should already have a visible sprite due to
            // the fact that we used the sprite tree query.
            if (container == null)
                return entities.Count != 0;

            var spriteQuery = GetEntityQuery<SpriteComponent>();
            for (var i = entities.Count - 1; i >= 0; i--)
            {
                if (!spriteQuery.TryGetComponent(entities[i], out var spriteComponent) || !spriteComponent.Visible)
                    entities.RemoveSwap(i);
            }

            return entities.Count != 0;
        }
    }

    /// <summary>
    /// An entity was examined on the client.
    /// </summary>
    public sealed class ClientExaminedEvent : EntityEventArgs
    {
        /// <summary>
        ///     The entity performing the examining.
        /// </summary>
        public readonly EntityUid Examiner;

        /// <summary>
        ///     Entity being examined, for broadcast event purposes.
        /// </summary>
        public readonly EntityUid Examined;

        public ClientExaminedEvent(EntityUid examined, EntityUid examiner)
        {
            Examined = examined;
            Examiner = examiner;
        }
    }
}
