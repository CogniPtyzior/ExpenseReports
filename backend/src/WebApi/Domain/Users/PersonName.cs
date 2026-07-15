// Defines a validated person name for users.
using WebApi.Core.Exceptions;

namespace WebApi.Domain.Users;

/// <summary>
/// Value object representing the first name and last name of a user.
/// </summary>
internal sealed class PersonName
{
    private PersonName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    private PersonName()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public static PersonName Create(string firstName, string lastName)
    {
        firstName = NormalizeRequired(firstName, "user.first_name_required", "First name is required.");
        lastName = NormalizeRequired(lastName, "user.last_name_required", "Last name is required.");

        return new PersonName(firstName, lastName);
    }

    public string FullName => $"{FirstName} {LastName}";

    private static string NormalizeRequired(string value, string code, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException(code, message);
        }

        return value.Trim();
    }
}