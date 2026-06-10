using Domain.Orders;
using Domain.Orders.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity Framework Core configuration for the <see cref="Order"/> aggregate root.
/// </summary>
public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(order => order.Id);

        builder.Property(order => order.Id)
            .HasConversion(id => id.Value, value => new OrderId(value))
            .ValueGeneratedNever();

        builder.Property(order => order.CustomerId)
            .HasConversion(id => id.Value, value => new CustomerId(value))
            .IsRequired();

        builder.Property(order => order.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(order => order.CreatedAtUtc)
            .IsRequired();

        builder.OwnsOne(order => order.ShippingAddress, address =>
        {
            address.Property(a => a.Street)
                .HasColumnName("ShippingStreet")
                .IsRequired();

            address.Property(a => a.City)
                .HasColumnName("ShippingCity")
                .IsRequired();

            address.Property(a => a.PostalCode)
                .HasColumnName("ShippingPostalCode")
                .IsRequired();

            address.Property(a => a.Country)
                .HasColumnName("ShippingCountry")
                .IsRequired();
        });

        builder.HasMany(order => order.Items)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetField("_items");
        builder.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(order => order.TotalAmount);
        builder.Ignore(order => order.DomainEvents);
    }
}
