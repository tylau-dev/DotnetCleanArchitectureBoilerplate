using Application.Common.Messaging;
using Application.Orders.Dtos;
using AutoMapper;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Queries;

/// <summary>
/// Query that retrieves an order by its identifier.
/// </summary>
/// <param name="OrderId">The identifier of the order to retrieve.</param>
public sealed record GetOrderByIdQuery(Guid OrderId) : IQuery<OrderDto?>;

/// <summary>
/// Validates <see cref="GetOrderByIdQuery"/> instances.
/// </summary>
public sealed class GetOrderByIdValidator : AbstractValidator<GetOrderByIdQuery>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrderByIdValidator"/> class.
    /// </summary>
    public GetOrderByIdValidator()
    {
        RuleFor(query => query.OrderId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="GetOrderByIdQuery"/> by retrieving and mapping the matching <see cref="Order"/>.
/// </summary>
public sealed class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    private readonly IOrderRepository orderRepository;
    private readonly IMapper mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetOrderByIdHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to load the requested order.</param>
    /// <param name="mapper">The mapper used to project the order onto an <see cref="OrderDto"/>.</param>
    public GetOrderByIdHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        this.orderRepository = orderRepository;
        this.mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken).ConfigureAwait(false);

        return order is null ? null : mapper.Map<OrderDto>(order);
    }
}
