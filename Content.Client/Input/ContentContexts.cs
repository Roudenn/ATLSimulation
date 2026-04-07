using Content.Shared.Input;
using Robust.Shared.Input;

namespace Content.Client.Input;

/// <summary>
///     Contains a helper function for setting up all content
///     contexts, and modifying existing engine ones.
/// </summary>
public static class ContentContexts
{
    public static void SetupContexts(IInputContextContainer contexts)
    {
        var common = contexts.GetContext("common");

        // Movement
        common.AddFunction(EngineKeyFunctions.MoveUp);
        common.AddFunction(EngineKeyFunctions.MoveDown);
        common.AddFunction(EngineKeyFunctions.MoveLeft);
        common.AddFunction(EngineKeyFunctions.MoveRight);
        common.AddFunction(EngineKeyFunctions.Walk);

        // UI and utilities
        common.AddFunction(ContentKeyFunctions.EscapeContext);
        common.AddFunction(ContentKeyFunctions.ExamineEntity);
        common.AddFunction(ContentKeyFunctions.TakeScreenshot);
        common.AddFunction(ContentKeyFunctions.TakeScreenshotNoUI);
        common.AddFunction(ContentKeyFunctions.ToggleFullscreen);
        common.AddFunction(ContentKeyFunctions.ZoomOut);
        common.AddFunction(ContentKeyFunctions.ZoomIn);
        common.AddFunction(ContentKeyFunctions.ResetZoom);
        common.AddFunction(ContentKeyFunctions.ToggleEntitySpawningWindow);
        common.AddFunction(ContentKeyFunctions.ToggleTileSpawningWindow);
        common.AddFunction(ContentKeyFunctions.InspectSubgridElement);
        common.AddFunction(ContentKeyFunctions.AdjustSubgridElement);
        common.AddFunction(ContentKeyFunctions.ReduceSubgridElement);

        // Editor
        common.AddFunction(ContentKeyFunctions.EditorCopyObject);
        common.AddFunction(ContentKeyFunctions.EditorFlipObject);
        common.AddFunction(EngineKeyFunctions.EditorRotateObject);
    }
}
