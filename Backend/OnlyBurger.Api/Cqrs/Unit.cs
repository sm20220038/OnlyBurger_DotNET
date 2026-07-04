namespace OnlyBurger.Api.Cqrs;

/// <summary>
/// Represents the absence of a meaningful return value, for commands whose effect is the
/// state change itself (e.g. delete). Lets such commands still flow through the generic
/// dispatcher, which always expects a result type.
/// </summary>
public readonly record struct Unit
{
    public static readonly Unit Value = new();
}
