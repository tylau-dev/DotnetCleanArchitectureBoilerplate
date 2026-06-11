using Application.Common.Exceptions;
using Application.Common.Messaging;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Commands;

/// <summary>
/// Command that ships an order, transitioning it from <see cref="Domain.Orders.OrderStatus.Placed"/>
/// to <see cref="Domain.Orders.OrderStatus.Shipped"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order to ship.</param>
public sealed record ShipOrderCommand(Guid OrderId) : ICommand;

/// <summary>
/// Validates <see cref="ShipOrderCommand"/> instances.
/// </summary>
public sealed class ShipOrderValidator : AbstractValidator<ShipOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ShipOrderValidator"/> class.
    /// </summary>
    public ShipOrderValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="ShipOrderCommand"/> by shipping the targeted <see cref="Order"/>.
/// </summary>
public sealed class ShipOrderHandler : IRequestHandler<ShipOrderCommand>
{
    private readonly IOrderRepository orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ShipOrderHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to load the targeted order.</param>
    public ShipOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    /// <inheritdoc />
    /// <exception cref="NotFoundException">Thrown when no order with the given identifier exists.</exception>
    public async Task Handle(ShipOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Ship();
    }
}
