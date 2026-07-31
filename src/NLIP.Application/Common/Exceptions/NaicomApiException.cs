namespace NLIP.Application.Common.Exceptions;

/// <summary>Thrown by INaicomApiClient implementations on unrecoverable failures (e.g. auth
/// rejected after refresh). Recoverable failures (timeouts, 5xx) are represented as a failed
/// NaicomApiResponse instead so the retry engine can act on them without exception overhead.</summary>
public class NaicomApiException : Exception
{
    public int? HttpStatusCode { get; }

    public NaicomApiException(string message, int? httpStatusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        HttpStatusCode = httpStatusCode;
    }
}
