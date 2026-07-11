namespace OnlyBurger.Domain.Enums;

/// <summary>
/// Lifecycle state of a shopping cart. A user builds their order in an <see cref="Active"/>
/// cart; at checkout that cart becomes <see cref="CheckedOut"/> and is linked to the order
/// it produced, so it is preserved as history rather than deleted.
/// </summary>
public enum CartStatus
{
    /// <summary>The user's current, editable cart. A user has at most one of these at a time.</summary>
    Active = 0,

    /// <summary>The cart was turned into an order at checkout and is now read-only history.</summary>
    CheckedOut = 1
}
