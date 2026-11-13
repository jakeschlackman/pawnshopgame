using Microsoft.Xna.Framework;
using PawnShop.Framework.Rendering;

namespace PawnShop.Characters.Player.Create;

public enum SelectionPose { LeaningIdle, PushOff, Approach, Flourish, SelectedIdle, TurnToTree, Return, TurnToCamera, Settle }

// Presentation state only. Foot travel is independent of frame playback; Draw never advances either.
public sealed class SelectionActor
{
    public static readonly TimeSpan WalkDuration = TimeSpan.FromMilliseconds(760);
    private readonly SpriteAnimator animator;
    private double progress;
    public PlayerGender Gender { get; }
    public SelectionPose Pose { get; private set; } = SelectionPose.LeaningIdle;
    public bool WantsForeground { get; internal set; }
    public double Progress => progress;
    public int Frame => animator.FrameIndex;
    public Vector2 Position => Vector2.Lerp(CharacterSelectionLayout.TreeFoot(Gender),
        CharacterSelectionLayout.ForegroundFoot(Gender), (float)progress);

    internal SelectionActor(PlayerGender gender, SpriteSheetDefinition sheet)
    {
        Gender = gender;
        animator = new(sheet, "LeaningIdle");
    }

    internal void Update(TimeSpan delta)
    {
        while (true)
        {
            // Finish short gestures/turns instead of resetting their pose when intent changes.
            if (Pose == SelectionPose.LeaningIdle && WantsForeground) Play(SelectionPose.PushOff);
            else if ((Pose is SelectionPose.Approach or SelectionPose.SelectedIdle) && !WantsForeground) Play(SelectionPose.TurnToTree);
            else if (Pose == SelectionPose.Return && WantsForeground) Play(SelectionPose.TurnToCamera);

            if (Pose is SelectionPose.LeaningIdle or SelectionPose.SelectedIdle)
            {
                animator.Update(delta);
                return;
            }
            var remaining = Pose switch
            {
                SelectionPose.Approach => TimeSpan.FromTicks((long)Math.Round((1 - progress) * WalkDuration.Ticks)),
                SelectionPose.Return => TimeSpan.FromTicks((long)Math.Round(progress * WalkDuration.Ticks)),
                _ => animator.Remaining
            };
            var step = delta < remaining ? delta : remaining;
            animator.Update(step);
            if (Pose == SelectionPose.Approach) progress = Math.Clamp(progress + (double)step.Ticks / WalkDuration.Ticks, 0, 1);
            if (Pose == SelectionPose.Return) progress = Math.Clamp(progress - (double)step.Ticks / WalkDuration.Ticks, 0, 1);
            delta -= step;
            if (step < remaining) return;
            switch (Pose)
            {
                case SelectionPose.PushOff:
                    Play(WantsForeground ? SelectionPose.Approach : SelectionPose.Settle);
                    break;
                case SelectionPose.Approach:
                    progress = 1;
                    Play(SelectionPose.Flourish);
                    break;
                case SelectionPose.Flourish:
                    Play(WantsForeground ? SelectionPose.SelectedIdle : SelectionPose.TurnToTree);
                    break;
                case SelectionPose.TurnToTree:
                    Play(SelectionPose.Return);
                    break;
                case SelectionPose.Return:
                    progress = 0;
                    Play(SelectionPose.TurnToCamera);
                    break;
                case SelectionPose.TurnToCamera:
                    Play(WantsForeground ? SelectionPose.Approach : progress == 0 ? SelectionPose.Settle : SelectionPose.TurnToTree);
                    break;
                case SelectionPose.Settle:
                    Play(SelectionPose.LeaningIdle);
                    break;
            }
        }
    }

    private void Play(SelectionPose pose)
    {
        Pose = pose;
        animator.Play(pose switch { SelectionPose.Approach => "WalkSouth", SelectionPose.Return => "WalkNorth", _ => pose.ToString() });
    }
}

public sealed class CharacterSelectionPresentation
{
    public CharacterSelectionCatalog Catalog { get; }
    public SelectionActor Male { get; private set; }
    public SelectionActor Female { get; private set; }
    public PlayerGender? Highlighted { get; private set; }
    public PlayerGender? Confirmation { get; private set; }
    public bool IsConfirming => Confirmation is not null;
    public bool ReadyToConfirm => Confirmation is { } gender
        && Actor(gender).Pose == SelectionPose.SelectedIdle
        && Actor(gender == PlayerGender.Male ? PlayerGender.Female : PlayerGender.Male).Pose == SelectionPose.LeaningIdle;

    public CharacterSelectionPresentation(CharacterSelectionCatalog catalog)
    {
        Catalog = catalog;
        Male = new(PlayerGender.Male, catalog.Sheets[PlayerGender.Male]);
        Female = new(PlayerGender.Female, catalog.Sheets[PlayerGender.Female]);
    }

    public SelectionActor Actor(PlayerGender gender) => gender == PlayerGender.Male ? Male : Female;

    public void Reset()
    {
        Male = new(PlayerGender.Male, Catalog.Sheets[PlayerGender.Male]);
        Female = new(PlayerGender.Female, Catalog.Sheets[PlayerGender.Female]);
        Highlighted = Confirmation = null;
    }

    public void Highlight(int index)
    {
        if (IsConfirming) return;
        Highlighted = index switch { 0 => PlayerGender.Male, 1 => PlayerGender.Female, _ => null };
        Male.WantsForeground = Highlighted == PlayerGender.Male;
        Female.WantsForeground = Highlighted == PlayerGender.Female;
    }

    public void Confirm(PlayerGender gender)
    {
        if (IsConfirming) return;
        Highlight(gender == PlayerGender.Male ? 0 : 1);
        Confirmation = gender;
    }

    public void Update(TimeSpan delta)
    {
        if (delta < TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(delta));
        Male.Update(delta);
        Female.Update(delta);
    }
}
