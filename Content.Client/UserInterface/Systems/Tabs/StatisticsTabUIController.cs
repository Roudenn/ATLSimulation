using Content.Client.Atmospherics;
using Content.Client.UserInterface.Systems.Tabs.Widgets;
using Content.Shared.Statistics;
using Content.Shared.Subgrid;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.UserInterface.Systems.Tabs;

[UsedImplicitly]
public sealed class StatisticsTabUIController : UIController
{
    [Dependency] private readonly ILocalizationManager _locale = default!;
    [UISystemDependency] private readonly AtmosphericsSystem _atmos = default!;

    private StatisticsTab? StatisticsTab => UIManager.GetActiveUIWidgetOrNull<GameTabContainer>()?.Statistics;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<StatisticsMessage>(OnGetStats);
        SubscribeLocalEvent<InspectSubGridAtmosphereTile>(OnInspectAtmos);
        SubscribeLocalEvent<InspectSubGridHeatTile>(OnInspectTemperature);
    }

    public void LoadButton()
    {
        if (StatisticsTab == null)
            return;

        PopulateStats(null);
    }

    public void UnloadButton()
    {
        if (StatisticsTab == null)
            return;

        //StatisticsTab.DebugSubGridChunksCheckBox.Pressed = false;
        //StatisticsTab.DebugSubGridChunksCheckBox.OnPressed -= DebugSubGridChunksOnPressed;
    }

    private void OnGetStats(StatisticsMessage msg, EntitySessionEventArgs ev)
    {
        PopulateStats(msg);
    }

    private void PopulateStats(StatisticsMessage? msg)
    {
        // I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE I HATE UI CODE
        var mapLabel = new RichTextLabel
        {
            Text = _locale.GetString("ui-viewport-tabs-statistics-current-map",
                ("value", msg?.Stats.CurrentMap ?? "Unknown"))
        };
        StatisticsTab?.MainInfoContainer.AddChild(mapLabel);

        var chunkLabel = new RichTextLabel
        {
            Text = _locale.GetString("ui-viewport-tabs-statistics-chunk-count",
                ("value", msg?.Stats.ChunkCount.ToString() ?? "Unknown"))
        };
        StatisticsTab?.MainInfoContainer.AddChild(chunkLabel);

        var tileLabel = new RichTextLabel
        {
            Text = _locale.GetString("ui-viewport-tabs-statistics-tile-count",
                ("value", msg?.Stats.TileCount.ToString() ?? "Unknown"))
        };
        StatisticsTab?.MainInfoContainer.AddChild(tileLabel);
    }

    private void OnInspectAtmos(ref InspectSubGridAtmosphereTile args)
    {
        var argsGasMixture = args.GasMixture;
        if (argsGasMixture == null
            || StatisticsTab == null)
            return;

        var entry = _atmos.GenerateGaxMixEntry(argsGasMixture.Value);
        StatisticsTab.GasInspector.Populate(entry);
        StatisticsTab.GasInspector.Visible = true;
        StatisticsTab.HeatInspector.Visible = false;
    }

    private void OnInspectTemperature(ref InspectSubGridHeatTile args)
    {
        if (args.HeatContainer == null
            || StatisticsTab == null)
            return;

        StatisticsTab.HeatInspector.Populate(args.HeatContainer.Value);
        StatisticsTab.HeatInspector.Visible = true;
        StatisticsTab.GasInspector.Visible = false;
    }
}
