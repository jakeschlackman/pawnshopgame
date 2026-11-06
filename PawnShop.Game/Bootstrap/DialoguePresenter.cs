using PawnShop.Framework.Input;
using PawnShop.Framework.UI;
using PawnShop.Narrative.Dialogue;

namespace PawnShop.Game.Bootstrap;

// Presentation timing stays outside the narrative runtime and uses the application's UI clock.
public sealed class DialoguePresenter
{
    private readonly DialogueSession session;
    private readonly SelectionModel selection = new();
    private readonly AppSettings settings;
    private IReadOnlyList<string> lines = Array.Empty<string>();
    private TimeSpan elapsed;
    private bool revealed;
    public DialogueView View => session.View;
    public int SelectedIndex => selection.Index;
    public int FirstVisibleChoice => SelectedIndex / 3 * 3;
    public int Page { get; private set; }
    public int PageCount => (lines.Count + 3) / 4;
    public int Columns => (int)(548 / (6 * settings.UiScale / 100f));
    public string PageText { get; private set; } = "";
    public int VisibleCharacters => revealed ? PageText.Length : settings.RevealedCharacters(elapsed, PageText.Length);
    public bool FullyRevealed => VisibleCharacters >= PageText.Length;
    public bool ChoicesVisible => FullyRevealed && Page == PageCount - 1 && View.Choices.Count > 0;

    public DialoguePresenter(DialogueSession session, AppSettings settings)
    {
        this.session = session;
        this.settings = settings;
        SetLine();
    }

    public void Update(InputFrame input, TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        if (View.Ended) return;
        if (input.Cancel) { session.End(DialogueEndReason.Cancelled); return; }
        bool wasRevealed = FullyRevealed;
        elapsed += delta;
        // Even if this update finishes the reveal, the same press cannot select a response.
        if (input.Confirm && !wasRevealed) { revealed = true; return; }
        if (!FullyRevealed) return;
        if (Page < PageCount - 1)
        {
            if (input.Confirm) { Page++; SetPage(); }
            return;
        }
        if (View.Choices.Count > 0)
        {
            if (!selection.Handle(input, View.Choices.Count)) return;
            var view = View;
            session.TryChoose(view.SessionId, view.Choices[SelectedIndex].Id, view.Revision);
            SetLine();
        }
        else if (input.Confirm)
        {
            var view = View;
            session.TryAdvance(view.SessionId, view.Revision);
            SetLine();
        }
    }

    private void SetLine()
    {
        lines = TextPages.Wrap(View.Text, Columns);
        Page = 0;
        selection.Reset();
        SetPage();
    }

    private void SetPage()
    {
        PageText = string.Join('\n', lines.Skip(Page * 4).Take(4));
        elapsed = TimeSpan.Zero;
        revealed = false;
    }
}
