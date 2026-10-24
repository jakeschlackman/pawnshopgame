using PawnShop.Framework.Input;
using PawnShop.Framework.UI;

namespace PawnShop.Tests;

internal static partial class RegressionTests
{
    private enum TestCommand { First, Second }

    internal static void Menus()
    {
        var items = new[] { new MenuItem<TestCommand>("A", TestCommand.First), new MenuItem<TestCommand>("B", TestCommand.Second) };
        var menu = new Menu<TestCommand>(items);
        items[0] = new("Changed", TestCommand.Second);
        Check(menu.Selected.Label == "A");
        Check(menu.Handle(new(Up: true, Confirm: true)) is null && menu.SelectedIndex == 1);
        menu.Handle(new(Down: true));
        Check(menu.SelectedIndex == 0);
        Check(menu.Handle(new(Confirm: true)) == TestCommand.First);
    }
    internal static void Edges()
    {
        var edges = new InputEdges();
        Check(edges.Read(InputButtons.Confirm).Confirm);
        Check(!edges.Read(InputButtons.Confirm).Confirm);
        edges.Read(InputButtons.None);
        Check(edges.Read(InputButtons.Confirm).Confirm);
        edges.Synchronize(InputButtons.Cancel | InputButtons.Interact);
        Check(edges.Read(InputButtons.Cancel | InputButtons.Interact) == default);
    }
}
