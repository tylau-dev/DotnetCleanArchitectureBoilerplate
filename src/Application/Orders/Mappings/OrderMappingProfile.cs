using Application.Orders.Dtos;
using AutoMapper;
using Domain.Orders;
using Domain.Orders.ValueObjects;

namespace Application.Orders.Mappings;

/// <summary>
/// AutoMapper profile mapping <see cref="Order"/> aggregates and their members to
/// Application-layer DTOs.
/// </summary>
public sealed class OrderMappingProfile : Profile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OrderMappingProfile"/> class, configuring
    /// all <see cref="Order"/>-related mappings.
    /// </summary>
    public OrderMappingProfile()
    {
        CreateMap<OrderId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<CustomerId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<ProductId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<OrderItemId, Guid>().ConvertUsing(id => id.Value);

        CreateMap<OrderStatus, OrderStatusDto>();

        CreateMap<Address, AddressDto>();

        CreateMap<OrderItem, OrderItemDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.UnitPrice.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.UnitPrice.Currency))
            .ForMember(dest => dest.Subtotal, opt => opt.MapFrom(src => src.Subtotal.Amount));

        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount.Amount))
            .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.TotalAmount.Currency));
    }
}
