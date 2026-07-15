// Defines a validated postal address for users.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.Users;

/// <summary>
/// Value object representing the postal address of a user.
/// </summary>
internal sealed class PostalAddress
{
    private PostalAddress(string street, string postalCode, string city)
    {
        Street = street;
        PostalCode = postalCode;
        City = city;
    }

    private PostalAddress()
    {
        Street = string.Empty;
        PostalCode = string.Empty;
        City = string.Empty;
    }

    public string Street { get; private set; }
    public string PostalCode { get; private set; }
    public string City { get; private set; }

    public static PostalAddress Create(string street, string postalCode, string city)
    {
        return new PostalAddress(
            NormalizeRequired(street, "user.street_required", "Street is required."),
            NormalizeRequired(postalCode, "user.postal_code_required", "Postal code is required."),
            NormalizeRequired(city, "user.city_required", "City is required."));
    }

    private static string NormalizeRequired(string value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(code, message);
        }

        return value.Trim();
    }
}