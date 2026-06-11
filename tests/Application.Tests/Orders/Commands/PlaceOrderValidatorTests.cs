using Application.Orders.Commands;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Commands;

public sealed class PlaceOrderValidatorTests
{
    private readonly PlaceOrderValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_OrderIdIsNotEmpty()
    {
        var result = validator.TestValidate(new PlaceOrderCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_OrderIdIsEmpty()
    {
        var result = validator.TestValidate(new PlaceOrderCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }
}
