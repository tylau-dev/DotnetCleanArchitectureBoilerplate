using Application.Orders.Commands;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Commands;

public sealed class ShipOrderValidatorTests
{
    private readonly ShipOrderValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_OrderIdIsNotEmpty()
    {
        var result = validator.TestValidate(new ShipOrderCommand(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_OrderIdIsEmpty()
    {
        var result = validator.TestValidate(new ShipOrderCommand(Guid.Empty));

        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }
}
