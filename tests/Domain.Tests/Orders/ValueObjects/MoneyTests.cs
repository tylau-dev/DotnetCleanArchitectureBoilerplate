using Domain.Orders.ValueObjects;

namespace Domain.Tests.Orders.ValueObjects;

public class MoneyTests
{
    [Fact]
    public void Should_BeEqual_When_AmountAndCurrencyMatch()
    {
        // Arrange
        var first = new Money(10.00m, "USD");
        var second = new Money(10.00m, "USD");

        // Act & Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Should_NotBeEqual_When_CurrencyDiffers()
    {
        // Arrange
        var first = new Money(10.00m, "USD");
        var second = new Money(10.00m, "EUR");

        // Act & Assert
        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Should_ReturnSum_When_AddingSameCurrency()
    {
        // Arrange
        var first = new Money(10.00m, "USD");
        var second = new Money(5.00m, "USD");

        // Act
        var result = first.Add(second);

        // Assert
        Assert.Equal(new Money(15.00m, "USD"), result);
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_AddingDifferentCurrencies()
    {
        // Arrange
        var first = new Money(10.00m, "USD");
        var second = new Money(5.00m, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => first.Add(second));
    }

    [Fact]
    public void Should_ThrowInvalidOperationException_When_SubtractingDifferentCurrencies()
    {
        // Arrange
        var first = new Money(10.00m, "USD");
        var second = new Money(5.00m, "EUR");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => first.Subtract(second));
    }
}
