namespace Sample.Interfaces.ValueObjects;

// "Soft" = may hold an invalid value on purpose, so a bad DTO still reaches the
// collect-all validator instead of failing at deserialization.
public record SoftGreeting(string Value)
{
    public static implicit operator string(SoftGreeting g) => g.Value;
    public static implicit operator SoftGreeting(string s) => new(s);
}
