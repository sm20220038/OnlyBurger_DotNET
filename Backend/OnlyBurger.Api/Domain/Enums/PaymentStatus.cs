namespace OnlyBurger.Api.Domain.Enums;

/// <summary>
/// Payment state of an order. Payment is simulated (no real gateway integration).
/// </summary>
public enum PaymentStatus
{
    Unpaid = 0,
    Paid = 1
}
