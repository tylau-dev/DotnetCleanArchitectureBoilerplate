using Application.Common.Exceptions;
using Application.Common.Messaging;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Commands;

/// <summary>
/// Command that places an order, transitioning it from <see cref="Domain.Orders.OrderStatus.Draft"/>
/// to <see cref="Domain.Orders.OrderStatus.Placed"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order to place.</param>
public sealed record PlaceOrderCommand(Guid OrderId) : ICommand;

/// <summary>
/// Validates <see cref="PlaceOrderCommand"/> instances.
/// </summary>
public sealed class PlaceOrderValidator : AbstractValidator<PlaceOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceOrderValidator"/> class.
    /// </summary>
    public PlaceOrderValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="PlaceOrderCommand"/> by placing the targeted <see cref="Order"/>.
/// </summary>
public sealed class PlaceOrderHandler : IRequestHandler<PlaceOrderCommand>
{
    private readonly IOrderRepository orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceOrderHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to load the targeted order.</param>
    public PlaceOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    /// <inheritdoc />
    /// <exception cref="NotFoundException">Thrown when no order with the given identifier exists.</exception>
    public async Task Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Place();
    }
}
