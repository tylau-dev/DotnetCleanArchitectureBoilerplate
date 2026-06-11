using Application.Orders.Queries;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Queries;

public sealed class GetOrderByIdValidatorTests
{
    private readonly GetOrderByIdValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_OrderIdIsNotEmpty()
    {
        var result = validator.TestValidate(new GetOrderByIdQuery(Guid.NewGuid()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_OrderIdIsEmpty()
    {
        var result = validator.TestValidate(new GetOrderByIdQuery(Guid.Empty));

        result.ShouldHaveValidationErrorFor(c => c.OrderId);
    }
}
