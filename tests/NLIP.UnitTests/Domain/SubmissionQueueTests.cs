using FluentAssertions;
using NLIP.Domain.Entities.Integration;
using Xunit;

namespace NLIP.UnitTests.Domain;

public class SubmissionQueueTests
{
    [Fact]
    public void RetryScheduleSeconds_MatchesSpecifiedBackoff()
    {
        SubmissionQueue.RetryScheduleSeconds.Should().Equal(30, 60, 300, 900, 1800, 3600);
    }
}
