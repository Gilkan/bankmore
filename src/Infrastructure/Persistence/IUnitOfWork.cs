using System.Data;

namespace BankMore.Infrastructure.Persistence;

public interface IUnitOfWork : IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }

    void Begin();
    void Commit();
    void Rollback();
}
