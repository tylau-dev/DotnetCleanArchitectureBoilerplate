using Application.Orders.Commands;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Commands;

public sealed class AddOrderItemValidatorTests
{
    private readonly AddOrderItemValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_CommandIsValid()
    {
        var command = new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Widget", 9.99m, "USD", 1);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_OrderIdIsEmpty()
    {
        var command = new AddOrderItemCommand(Guid.Empty, Guid.NewGuid(), "Widget", 9.99m, "USD", 1);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }

    [Fact]
    public void Should_HaveValidationError_When_QuantityIsNotPositive()
    {
        var command = new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Widget", 9.99m, "USD", 0);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Quantity);
    }

    [Fact]
    public void Should_HaveValidationError_When_UnitPriceIsNegative()
    {
        var command = new AddOrderItemCommand(Guid.NewGuid(), Guid.NewGuid(), "Widget", -1m, "USD", 1);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.UnitPrice);
    }
}
