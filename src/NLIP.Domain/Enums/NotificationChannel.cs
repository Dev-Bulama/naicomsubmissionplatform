namespace NLIP.Domain.Enums;

public enum NotificationChannel
{
    InApp = 1,
    SignalR = 2,
    Email = 3,
    Sms = 4,
    Teams = 5,
    Slack = 6
}

public enum NotificationStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2
}
