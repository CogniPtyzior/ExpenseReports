// Defines the transaction boundary used by write use cases.
namespace WebApi.Application.Core.Persistence;

/// <summary>
/// Persists changes made through repositories during an application use case.
/// </summary>
internal interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken);
}