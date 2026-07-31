namespace NLIP.Shared.Constants;

/// <summary>Keys stored in the SystemSettings table, editable from the admin Settings module.</summary>
public static class SettingKeys
{
    public const string NaicomBaseUrl = "Naicom:BaseUrl";
    public const string NaicomSid = "Naicom:Sid";
    public const string NaicomSecret = "Naicom:Secret";
    public const string NaicomTimeoutSeconds = "Naicom:TimeoutSeconds";

    public const string RetryMaxAttempts = "Retry:MaxAttempts";
    public const string RetrySchedule = "Retry:ScheduleCsv";

    public const string QueueMaxConcurrency = "Queue:MaxConcurrency";
    public const string LoggingMinimumLevel = "Logging:MinimumLevel";

    public const string EmailSmtpHost = "Email:SmtpHost";
    public const string EmailSmtpPort = "Email:SmtpPort";
    public const string EmailFromAddress = "Email:FromAddress";

    public const string SmsProviderEndpoint = "Sms:ProviderEndpoint";

    public const string SessionTimeoutMinutes = "Security:SessionTimeoutMinutes";
    public const string AccountLockoutThreshold = "Security:AccountLockoutThreshold";
    public const string AccountLockoutMinutes = "Security:AccountLockoutMinutes";
}
