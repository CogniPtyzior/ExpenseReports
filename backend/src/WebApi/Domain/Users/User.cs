// Defines the user aggregate and its assignability rules.
namespace WebApi.Domain.Users;

/// <summary>
/// User aggregate representing a company member assignable to expense reports.
/// </summary>
internal sealed class User
{
    private User()
    {
        Name = null!;
        Address = null!;
    }

    private User(
        Guid id,
        PersonName name,
        PostalAddress address,
        MonthlyExpenseQuota monthlyExpenseQuota,
        DateTime createdAtUtc)
    {
        Id = id;
        Name = name;
        Address = address;
        MonthlyExpenseQuota = monthlyExpenseQuota;
        IsActive = true;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public PersonName Name { get; private set; }
    public PostalAddress Address { get; private set; }
    public MonthlyExpenseQuota MonthlyExpenseQuota { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public DateTime? DeletedAtUtc { get; private set; }

    public bool CanBeAssignedToExpenseReport => IsActive && !IsDeleted;

    public static User Create(
        Guid id,
        PersonName name,
        PostalAddress address,
        MonthlyExpenseQuota monthlyExpenseQuota,
        DateTime createdAtUtc)
    {
        return new User(id, name, address, monthlyExpenseQuota, createdAtUtc);
    }

    public void Update(PersonName name, PostalAddress address, MonthlyExpenseQuota monthlyExpenseQuota, DateTime updatedAtUtc)
    {
        Name = name;
        Address = address;
        MonthlyExpenseQuota = monthlyExpenseQuota;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Activate(DateTime updatedAtUtc)
    {
        IsActive = true;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void Deactivate(DateTime updatedAtUtc)
    {
        IsActive = false;
        UpdatedAtUtc = updatedAtUtc;
    }

    public void SoftDelete(DateTime deletedAtUtc)
    {
        IsDeleted = true;
        DeletedAtUtc = deletedAtUtc;
        UpdatedAtUtc = deletedAtUtc;
    }
}