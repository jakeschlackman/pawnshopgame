# Working on player creation
Start with PlayerModel_SelectionScreen.cs. CharacterSelectionLayout in the same file shares card, Back, selector, and flight-anchor geometry so drawing and pointer input agree.

Game/Bootstrap/ApplicationFlow owns startup navigation, travel input capture, and session creation. Game/Screens/GameScreens composes the forest background, original models, and Sprout overlay. MenuPointerInput consumes the shared bounds.

Selection stays sessionless until a model is confirmed. Sprout shares the existing pointing animator and follows the camera's eased progress; no extra animation clock or artwork is required for UI wiring.

After a code change use the [root build/test commands](../../../README.md#build-and-run). Relevant regressions are in PlayerTests, LifecycleTests, PanoramaTests, and MenuMusicTests. Check model/selector alignment, camera feel, keyboard/mouse selection, focus behavior, and music manually; automated checks do not establish those visual/listening results.
