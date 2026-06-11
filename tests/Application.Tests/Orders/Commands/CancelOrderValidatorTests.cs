using Application.Orders.Commands;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Commands;

public sealed class CancelOrderValidatorTests
{
    private readonly CancelOrderValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_OrderIdIsNotEmpty()
    {
        var result = validator.TestValidate(new CancelOrderCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_OrderIdIsEmpty()
    {
        var result = validator.TestValidate(new CancelOrderCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }
}
