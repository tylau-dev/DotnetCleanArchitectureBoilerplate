using Domain.Common;

namespace Domain.Tests.Common;

public class ValueObjectTests
{
    [Fact]
    public void Should_BeEqual_When_AllComponentsMatch()
    {
        // Arrange
        var first = new TestValueObject("widget", 1);
        var second = new TestValueObject("widget", 1);

        // Act & Assert
        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Should_NotBeEqual_When_AnyComponentDiffers()
    {
        // Arrange
        var first = new TestValueObject("widget", 1);
        var second = new TestValueObject("gadget", 1);

        // Act & Assert
        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void Should_NotBeEqual_When_ComparedToNull()
    {
        // Arrange
        var value = new TestValueObject("widget", 1);

        // Act & Assert
        Assert.False(value.Equals(null));
        Assert.True(value != null);
    }

    private sealed class TestValueObject : ValueObject
    {
        public TestValueObject(string name, int quantity)
        {
            Name = name;
            Quantity = quantity;
        }

        public string Name { get; }

        public int Quantity { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Name;
            yield return Quantity;
        }
    }
}
