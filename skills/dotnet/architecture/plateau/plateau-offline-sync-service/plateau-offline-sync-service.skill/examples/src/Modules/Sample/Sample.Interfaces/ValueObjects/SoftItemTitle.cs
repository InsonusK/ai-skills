namespace Sample.Interfaces.ValueObjects;

// "Soft" = may hold an invalid value on purpose, so a bad DTO still reaches the validator.
public record SoftItemTitle(string Value)
{
    public static implicit operator string(SoftItemTitle t) => t.Value;
    public static implicit operator SoftItemTitle(string s) => new(s);
}
