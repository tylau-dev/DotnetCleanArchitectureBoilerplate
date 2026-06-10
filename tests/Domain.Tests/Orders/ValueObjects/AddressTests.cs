using Domain.Orders.ValueObjects;

namespace Domain.Tests.Orders.ValueObjects;

public class AddressTests
{
    [Fact]
    public void Should_BeEqual_When_AllComponentsMatch()
    {
        // Arrange
        var first = new Address("123 Main St", "Springfield", "12345", "USA");
        var second = new Address("123 Main St", "Springfield", "12345", "USA");

        // Act & Assert
        Assert.Equal(first, second);
    }

    [Fact]
    public void Should_NotBeEqual_When_AnyComponentDiffers()
    {
        // Arrange
        var first = new Address("123 Main St", "Springfield", "12345", "USA");
        var second = new Address("456 Elm St", "Springfield", "12345", "USA");

        // Act & Assert
        Assert.NotEqual(first, second);
    }
}
