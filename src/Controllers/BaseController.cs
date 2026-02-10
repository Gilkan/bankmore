using Microsoft.AspNetCore.Mvc;
using BankMore.Domain;
using Microsoft.Extensions.Options;
using BankMore.Infrastructure.Options;
using System.Security.Claims;
using MediatR;

namespace BankMore.Controllers
{
    [ApiController]
    public abstract class BaseController<TDependency> : ControllerBase
    {
        protected readonly TDependency _dependency;
        protected readonly bool _useStringGuids;

        protected BaseController(TDependency dependency, IOptions<DatabaseOptions> dbOptions)
        {
            _dependency = dependency;
            _useStringGuids = dbOptions.Value.UseStringGuids;
        }

        protected object GetContaIdFromToken()
        {
            var claim = User.Claims.FirstOrDefault(c => c.Type == "idContaCorrente");
            if (claim is null)
                throw new DomainException("Conta inválida", "INVALID_ACCOUNT");

            return _useStringGuids ? claim.Value : Guid.Parse(claim.Value);
        }
    }
}
