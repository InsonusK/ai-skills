namespace MaskedLogging.Tests.Steps;

/// <summary>
/// "Masking is switched off" of a scenario, shared by the masking steps and
/// the record steps through Reqnroll's context injection.
/// </summary>
public sealed class MaskingSwitch
{
    public bool Off { get; set; }
}
