using Application.Common.Behaviors;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Common.Behaviors;

public sealed class LoggingBehaviorTests
{
    public sealed record TestRequest : IRequest<string>;

    [Fact]
    public async Task Should_CallNext_When_Handling()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger.Object);

        var nextCalled = false;
        Task<string> Next(CancellationToken cancellationToken)
        {
            nextCalled = true;
            return Task.FromResult("response");
        }

        var result = await behavior.Handle(new TestRequest(), Next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("response", result);
    }

    [Fact]
    public async Task Should_LogAndRethrow_When_NextThrows()
    {
        var logger = new Mock<ILogger<LoggingBehavior<TestRequest, string>>>();
        var behavior = new LoggingBehavior<TestRequest, string>(logger.Object);

        var expectedException = new InvalidOperationException("boom");
        Task<string> Next(CancellationToken cancellationToken) => throw expectedException;

        var actualException = await Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(new TestRequest(), Next, CancellationToken.None));

        Assert.Same(expectedException, actualException);
    }
}
