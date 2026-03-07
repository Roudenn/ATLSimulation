using Content.Shared.Statistics;
using Robust.Shared.Timing;

namespace Content.Server.Statistics;

/// <summary>
/// Collects some statistics about the simulation and sends it over to clients.
/// </summary>
public sealed class StatisticsSystem : SharedStatisticsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public SimulationStats? ArchivedStatistics;

    private TimeSpan _nextUpdate = TimeSpan.Zero;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextUpdate >= _timing.CurTime)
            return;

        _nextUpdate = _timing.CurTime + TimeSpan.FromSeconds(1);

        // Message is sent only if something had changed
        var stats = GetStatistics();
        if (ArchivedStatistics.Equals(stats))
            return;

        ArchivedStatistics = stats;
        var ev = new StatisticsMessage(stats);
        RaiseNetworkEvent(ev);
    }

    private SimulationStats GetStatistics()
    {
        var ev = new GetStatisticsEvent();
        RaiseLocalEvent(ref ev);
        return ev.Stats;
    }
}


[ByRefEvent]
public record struct GetStatisticsEvent()
{
    public SimulationStats Stats = new();
}
