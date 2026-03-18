#nullable enable
using System.Linq;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Utility;

namespace Content.Shared.IntegrationTests;

// Contains misc helper functions to make writing tests easier.
public sealed partial class TestPair
{
    public Task<TestMapData> CreateTestMap(bool initialized = true)
        => CreateTestMap(initialized, "Plating");

    /// <summary>
    /// Loads a test map and returns a <see cref="TestMapData"/> representing it.
    /// </summary>
    /// <param name="testMapPath">The <see cref="ResPath"/> to the test map to load.</param>
    /// <param name="initialized">Whether to initialize the map on load.</param>
    /// <returns>A <see cref="TestMapData"/> representing the loaded map.</returns>
    public async Task<TestMapData> LoadTestMap(ResPath testMapPath, bool initialized = true)
    {
        TestMapData mapData = new();
        var deserializationOptions = DeserializationOptions.Default with { InitializeMaps = initialized };
        var mapLoaderSys = Server.EntMan.System<MapLoaderSystem>();
        var mapSys = Server.System<SharedMapSystem>();

        // Load our test map in and assert that it exists.
        await Server.WaitAssertion(() =>
        {
            Assert.That(mapLoaderSys.TryLoadMap(testMapPath, out var map, out var gridSet, deserializationOptions),
                $"Failed to load map {testMapPath}.");
            Assert.That(gridSet, Is.Not.Empty, "There were no grids loaded from the map!");

            mapData.MapUid = map!.Value.Owner;
            mapData.MapId = map!.Value.Comp.MapId;
            mapData.Grid = gridSet!.First();
            mapData.GridCoords = new EntityCoordinates(mapData.Grid, 0, 0);
            mapData.MapCoords = new MapCoordinates(0, 0, mapData.MapId);
            mapData.Tile = mapSys.GetAllTiles(mapData.Grid.Owner, mapData.Grid.Comp).First();
        });

        await RunTicksSync(10);
        mapData.CMapUid = ToClientUid(mapData.MapUid);
        mapData.CGridUid = ToClientUid(mapData.Grid);
        mapData.CGridCoords = new EntityCoordinates(mapData.CGridUid, 0, 0);

        return mapData;
    }
}
