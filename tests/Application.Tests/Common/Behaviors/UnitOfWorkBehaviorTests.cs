using Application.Common.Behaviors;
using Application.Common.Messaging;
using Domain.Common;
using MediatR;
using Moq;

namespace Application.Tests.Common.Behaviors;

public sealed class UnitOfWorkBehaviorTests
{
    public sealed record TestCommand : ICommand<string>;

    public sealed record TestQuery : IQuery<string>;

    [Fact]
    public async Task Should_CallSaveChangesAsync_When_RequestIsCommand()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestCommand, string>(unitOfWork.Object);

        Task<string> Next(CancellationToken cancellationToken) => Task.FromResult("response");

        var result = await behavior.Handle(new TestCommand(), Next, CancellationToken.None);

        Assert.Equal("response", result);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_NotCallSaveChangesAsync_When_RequestIsQuery()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var behavior = new UnitOfWorkBehavior<TestQuery, string>(unitOfWork.Object);

        Task<string> Next(CancellationToken cancellationToken) => Task.FromResult("response");

        var result = await behavior.Handle(new TestQuery(), Next, CancellationToken.None);

        Assert.Equal("response", result);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
