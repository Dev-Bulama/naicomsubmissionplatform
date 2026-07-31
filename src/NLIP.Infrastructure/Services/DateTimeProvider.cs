using NLIP.Application.Common.Interfaces;

namespace NLIP.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
