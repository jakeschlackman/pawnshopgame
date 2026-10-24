using PawnShop.Tests;

// Dependency-free executable regression suite. A failure exits nonzero.
var tests = new (string Name, Action Run)[]
{
    ("Title has no session; disabled controls explain and reject activation", RegressionTests.Title),
    ("Menu wraps, owns its items and separates navigation from activation", RegressionTests.Menus),
    ("Input edges reject held/replayed keys", RegressionTests.Edges),
    ("New game consumes confirm and creates exactly one session", RegressionTests.Start),
    ("Pause and both resume routes consume world input and freeze time", RegressionTests.Pause),
    ("Focus loss pauses and never advances the world", RegressionTests.Focus),
    ("Return cancellation preserves state; discard disposes and restarts fresh", RegressionTests.Return),
    ("Ten cycles keep one live session and dispose all previous sessions", RegressionTests.Cycles),
    ("Exit is distinct, cancellable in play, and disposes on confirmation", RegressionTests.Exit),
    ("Application disposal is idempotent and rejects subsequent updates", RegressionTests.Disposal)
};
int failures = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception e) { failures++; Console.Error.WriteLine($"FAIL {test.Name}: {e}"); }
}
Console.WriteLine($"{tests.Length - failures}/{tests.Length} tests passed.");
return failures == 0 ? 0 : 1;
