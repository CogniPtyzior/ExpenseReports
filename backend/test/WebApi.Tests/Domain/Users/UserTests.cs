// Covers the user aggregate and user value object invariants.
using WebApi.Core.Exceptions;
using WebApi.Domain.Users;

namespace WebApi.Tests.Domain.Users;

public sealed class UserTests
{
    private static readonly DateTime Now = new(2025, 10, 15, 8, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void Person_name_is_trimmed_and_exposes_full_name()
    {
        var name = PersonName.Create(" Juste ", " Leblanc ");

        Assert.Equal("Juste", name.FirstName);
        Assert.Equal("Leblanc", name.LastName);
        Assert.Equal("Juste Leblanc", name.FullName);
    }

    [Fact]
    public void Person_name_rejects_blank_parts()
    {
        Assert.Throws<DomainException>(() => PersonName.Create("", "Leblanc"));
        Assert.Throws<DomainException>(() => PersonName.Create("Juste", " "));
    }

    [Fact]
    public void Postal_address_requires_all_parts()
    {
        var address = PostalAddress.Create(" 1 Rue des Notes ", " 75001 ", " Paris ");

        Assert.Equal("1 Rue des Notes", address.Street);
        Assert.Equal("75001", address.PostalCode);
        Assert.Equal("Paris", address.City);
        Assert.Throws<DomainException>(() => PostalAddress.Create("", "75001", "Paris"));
    }

    [Fact]
    public void Monthly_expense_quota_must_be_positive()
    {
        Assert.Equal(5, MonthlyExpenseQuota.Create(5).Value);
        Assert.Throws<DomainException>(() => MonthlyExpenseQuota.Create(0));
    }

    [Fact]
    public void Created_user_is_active_and_assignable()
    {
        var user = CreateUser();

        Assert.Equal(Now, user.CreatedAtUtc);
        Assert.True(user.IsActive);
        Assert.False(user.IsDeleted);
        Assert.True(user.CanBeAssignedToExpenseReport);
    }

    [Fact]
    public void Deactivated_user_is_not_assignable()
    {
        var user = CreateUser();

        user.Deactivate(Now.AddMinutes(5));

        Assert.False(user.IsActive);
        Assert.False(user.CanBeAssignedToExpenseReport);
        Assert.Equal(Now.AddMinutes(5), user.UpdatedAtUtc);
    }

    [Fact]
    public void Soft_deleted_user_is_not_assignable_and_keeps_deleted_timestamp()
    {
        var user = CreateUser();

        user.SoftDelete(Now.AddMinutes(10));

        Assert.True(user.IsDeleted);
        Assert.False(user.CanBeAssignedToExpenseReport);
        Assert.Equal(Now.AddMinutes(10), user.DeletedAtUtc);
        Assert.Equal(Now.AddMinutes(10), user.UpdatedAtUtc);
    }

    [Fact]
    public void Update_replaces_profile_and_tracks_timestamp()
    {
        var user = CreateUser();

        user.Update(
            PersonName.Create("Marc", "Assin"),
            PostalAddress.Create("24 Avenue des Frais", "69002", "Lyon"),
            MonthlyExpenseQuota.Create(8),
            Now.AddHours(1));

        Assert.Equal("Marc", user.Name.FirstName);
        Assert.Equal("Assin", user.Name.LastName);
        Assert.Equal("24 Avenue des Frais", user.Address.Street);
        Assert.Equal(8, user.MonthlyExpenseQuota.Value);
        Assert.Equal(Now.AddHours(1), user.UpdatedAtUtc);
    }

    private static User CreateUser()
    {
        return User.Create(
            Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
            PersonName.Create("Juste", "Leblanc"),
            PostalAddress.Create("1 Rue des Notes", "75001", "Paris"),
            MonthlyExpenseQuota.Create(5),
            Now);
    }
}