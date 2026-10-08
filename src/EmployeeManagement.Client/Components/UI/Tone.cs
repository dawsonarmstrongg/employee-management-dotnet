namespace EmployeeManagement.Client.Components.UI;

// Meaning of a chip, alert, toast or state. Each tone pairs a color with an icon so status is never shown by color alone.
public enum Tone
{
    Neutral,
    Info,
    Success,
    Warning,
    Error,

    // The rare Solar Yellow highlight (for example "New").
    Attention,
}

internal static class ToneStyles
{
    public static string CssSuffix(Tone tone) => tone.ToString().ToLowerInvariant();

    public static IconName Icon(Tone tone) => tone switch
    {
        Tone.Success => IconName.CheckCircle,
        Tone.Warning => IconName.Warning,
        Tone.Error => IconName.ErrorCircle,
        Tone.Attention => IconName.Star,
        _ => IconName.Info,
    };
}