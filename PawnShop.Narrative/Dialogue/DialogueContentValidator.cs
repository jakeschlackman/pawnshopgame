namespace PawnShop.Narrative.Dialogue;

public static class DialogueContentValidator
{
    // A bounded branch smoke check for authoring. Visit counts and random-stream state
    // are not part of the key: unusual count/random-dependent paths still need authored tests.
    public static int Check(DialogueDefinition definition, int maximumStates = 256)
    {
        if (maximumStates < 1) throw new ArgumentOutOfRangeException(nameof(maximumStates));
        var pending = new Queue<DialogueSession>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        pending.Enqueue(new(definition));
        try
        {
            while (pending.TryDequeue(out var session))
            {
                using (session)
                {
                    if (session.View.Ended || !seen.Add(session.ValidationKey())) continue;
                    if (seen.Count + pending.Count > maximumStates)
                        throw definition.Invalid("validation", "Branch-check budget exceeded. Simplify the loop or add a focused branch test.");
                    if (session.View.Choices.Count == 0)
                    {
                        session.TryAdvance(session.View.SessionId, session.View.Revision);
                        if (!session.View.Ended) pending.Enqueue(session.ForkForValidation());
                    }
                    else
                    {
                        foreach (var choice in session.View.Choices)
                        {
                            if (seen.Count + pending.Count >= maximumStates)
                                throw definition.Invalid("validation", "Branch-check budget exceeded. Add a focused branch test.");
                            var branch = session.ForkForValidation();
                            try { branch.TryChoose(branch.View.SessionId, choice.Id, branch.View.Revision); }
                            catch { branch.Dispose(); throw; }
                            pending.Enqueue(branch);
                        }
                    }
                }
            }
            return seen.Count;
        }
        finally { foreach (var session in pending) session.Dispose(); }
    }
}
