using Robust.Shared.Input;

namespace Content.Shared.Input;

[KeyFunctions]
public static class ContentKeyFunctions
{ 
        public static readonly BoundKeyFunction ExamineEntity = "ExamineEntity";
        public static readonly BoundKeyFunction EscapeContext = "EscapeContext";
        public static readonly BoundKeyFunction TakeScreenshot = "TakeScreenshot";
        public static readonly BoundKeyFunction TakeScreenshotNoUI = "TakeScreenshotNoUI";
        public static readonly BoundKeyFunction ToggleFullscreen = "ToggleFullscreen";
        public static readonly BoundKeyFunction ZoomOut = "ZoomOut";
        public static readonly BoundKeyFunction ZoomIn = "ZoomIn"; 
        public static readonly BoundKeyFunction ResetZoom = "ResetZoom";
        public static readonly BoundKeyFunction EditorCopyObject = "EditorCopyObject";
        public static readonly BoundKeyFunction EditorFlipObject = "EditorFlipObject";
        public static readonly BoundKeyFunction ToggleStatsSummaryWindow = "ToggleStatsSummaryWindow";
}
