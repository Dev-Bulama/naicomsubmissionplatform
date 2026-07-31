namespace NLIP.Domain.Enums;

/// <summary>Which NAICOM Life Assurance endpoint a submission/transaction targets.</summary>
public enum NaicomAction
{
    Create = 1,
    Update = 2,
    Renew = 3,
    Terminate = 4,
    Query = 5,
    Delete = 6
}
