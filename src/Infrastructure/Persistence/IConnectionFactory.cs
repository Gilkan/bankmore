using System.Data;

namespace BankMore.Infrastructure.Persistence;

public interface IConnectionFactory
{
    IDbConnection Create();
}
