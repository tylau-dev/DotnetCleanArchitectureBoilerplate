using Application.Orders.Commands;
using FluentValidation.TestHelper;

namespace Application.Tests.Orders.Commands;

public sealed class CreateOrderValidatorTests
{
    private readonly CreateOrderValidator validator = new();

    [Fact]
    public void Should_NotHaveValidationError_When_CommandIsValid()
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), "123 Main St", "Springfield", "12345", "USA");

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveValidationError_When_CustomerIdIsEmpty()
    {
        var command = new CreateOrderCommand(Guid.Empty, "123 Main St", "Springfield", "12345", "USA");

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.CustomerId);
    }

    [Theory]
    [InlineData("", "Springfield", "12345", "USA")]
    [InlineData("123 Main St", "", "12345", "USA")]
    [InlineData("123 Main St", "Springfield", "", "USA")]
    [InlineData("123 Main St", "Springfield", "12345", "")]
    public void Should_HaveValidationError_When_AddressFieldIsEmpty(string street, string city, string postalCode, string country)
    {
        var command = new CreateOrderCommand(Guid.NewGuid(), street, city, postalCode, country);

        var result = validator.TestValidate(command);

        Assert.False(result.IsValid);
    }
}
