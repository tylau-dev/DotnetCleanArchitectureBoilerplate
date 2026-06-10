using Application.Common.Behaviors;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;

namespace Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record TestRequest(string Name) : IRequest<string>;

    [Fact]
    public async Task Should_CallNext_When_NoValidatorsRegistered()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);

        var nextCalled = false;
        Task<string> Next(CancellationToken cancellationToken)
        {
            nextCalled = true;
            return Task.FromResult("response");
        }

        var result = await behavior.Handle(new TestRequest("valid"), Next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("response", result);
    }

    [Fact]
    public async Task Should_CallNext_When_ValidationSucceeds()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);

        var nextCalled = false;
        Task<string> Next(CancellationToken cancellationToken)
        {
            nextCalled = true;
            return Task.FromResult("response");
        }

        var result = await behavior.Handle(new TestRequest("valid"), Next, CancellationToken.None);

        Assert.True(nextCalled);
        Assert.Equal("response", result);
    }

    [Fact]
    public async Task Should_ThrowValidationException_When_ValidationFails()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Name", "Name is required.")]));

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);

        Task<string> Next(CancellationToken cancellationToken) => Task.FromResult("response");

        await Assert.ThrowsAsync<ValidationException>(
            () => behavior.Handle(new TestRequest(string.Empty), Next, CancellationToken.None));
    }
}
