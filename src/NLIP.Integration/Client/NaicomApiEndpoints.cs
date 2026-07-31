namespace NLIP.Integration.Client;

/// <summary>Every NAICOM route in one place — see Dtos/README_VERIFY_AGAINST_SPEC.md. Update
/// these constants (not scattered strings) once the live spec is confirmed.</summary>
public static class NaicomApiEndpoints
{
    public const string Authenticate = "api/auth/token";
    public const string Health = "api/health";
    public const string Version = "api/version";

    public const string IndividualLifeCreate = "api/life/individual/policy";
    public const string IndividualLifeUpdate = "api/life/individual/policy/{0}";
    public const string IndividualLifeRenew = "api/life/individual/policy/{0}/renew";
    public const string IndividualLifeTerminate = "api/life/individual/policy/{0}/terminate";
    public const string IndividualLifeQuery = "api/life/individual/policy/{0}";
    public const string IndividualLifeDelete = "api/life/individual/policy/{0}";

    public const string GroupLifeCreate = "api/life/group/policy";
    public const string GroupLifeUpdate = "api/life/group/policy/{0}";
    public const string GroupLifeRenew = "api/life/group/policy/{0}/renew";
    public const string GroupLifeTerminate = "api/life/group/policy/{0}/terminate";
    public const string GroupLifeQuery = "api/life/group/policy/{0}";
    public const string GroupLifeDelete = "api/life/group/policy/{0}";
}
