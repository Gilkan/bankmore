namespace BankMore.Accounts.Api.Domain;

public class DomainException : Exception
{
    public string ErrorType { get; }

    public DomainException(string message, string errorType)
        : base(message)
    {
        ErrorType = errorType;
    }
}
