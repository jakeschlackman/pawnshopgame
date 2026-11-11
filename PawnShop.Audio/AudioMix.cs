namespace PawnShop.Audio;

public enum AudioChannel { Music, Ambient, Effects, Combat, Monster }

// A graphics-free snapshot of application preferences. UI and environmental
// asset folders use the existing Effects and Ambient mix channels respectively.
public readonly record struct AudioMix(int Master, int Music, int Ambient, int Effects,
    int Combat, int Monster, bool MuteUnfocused)
{
    public float Gain(AudioChannel channel, bool active) => !active && MuteUnfocused ? 0
        : Master / 100f * (channel switch
        {
            AudioChannel.Music => Music, AudioChannel.Ambient => Ambient,
            AudioChannel.Combat => Combat, AudioChannel.Monster => Monster, _ => Effects
        }) / 100f;
}
