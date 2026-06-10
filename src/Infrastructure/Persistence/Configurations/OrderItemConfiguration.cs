using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="OrderItem"/> entity.
/// </summary>
public sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(item => item.Id);

        builder.Property(item => item.Id)
            .HasConversion(id => id.Value, value => new OrderItemId(value))
            .ValueGeneratedNever();

        builder.Property(item => item.ProductId)
            .HasConversion(id => id.Value, value => new ProductId(value))
            .IsRequired();

        builder.Property(item => item.ProductName)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(item => item.Quantity)
            .IsRequired();

        builder.OwnsOne(item => item.UnitPrice, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("UnitPriceAmount")
                .HasColumnType("numeric(18,2)")
                .IsRequired();

            money.Property(m => m.Currency)
                .HasColumnName("UnitPriceCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Ignore(item => item.Subtotal);

        builder.Property<OrderId>("OrderId")
            .HasConversion(id => id.Value, value => new OrderId(value));
    }
}
