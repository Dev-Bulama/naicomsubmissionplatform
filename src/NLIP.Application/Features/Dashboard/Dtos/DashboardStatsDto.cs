namespace NLIP.Application.Features.Dashboard.Dtos;

public class DashboardStatsDto
{
    public int TotalPolicies { get; set; }
    public int PoliciesSubmittedToday { get; set; }
    public int RetailPolicies { get; set; }
    public int GroupPolicies { get; set; }

    public int SuccessfulSubmissions { get; set; }
    public int FailedSubmissions { get; set; }
    public int PendingSubmissions { get; set; }
    public int RetryQueueCount { get; set; }
    public int ProcessingQueueCount { get; set; }

    public double AverageResponseTimeMs { get; set; }
    public double SuccessRatePercent { get; set; }
    public double FailureRatePercent { get; set; }

    public List<VolumePointDto> DailyVolume { get; set; } = new();
    public List<VolumePointDto> WeeklyVolume { get; set; } = new();
    public List<VolumePointDto> MonthlyVolume { get; set; } = new();

    public bool NaicomApiAvailable { get; set; }
    public bool NaicomAuthenticated { get; set; }
    public string? NaicomVersion { get; set; }

    public List<TopErrorDto> TopErrors { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
}

public class VolumePointDto
{
    public string Label { get; set; } = default!;
    public int Count { get; set; }
}

public class TopErrorDto
{
    public string ErrorMessage { get; set; } = default!;
    public int Count { get; set; }
}

public class RecentActivityDto
{
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public DateTimeOffset Timestamp { get; set; }
}
