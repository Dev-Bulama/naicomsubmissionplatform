namespace NLIP.Domain.Exceptions;

/// <summary>Thrown when a domain invariant/business rule is violated inside an entity method.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
