using Application.Common.Exceptions;
using Application.Common.Messaging;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Commands;

/// <summary>
/// Command that cancels an order, transitioning it to <see cref="Domain.Orders.OrderStatus.Cancelled"/>.
/// </summary>
/// <param name="OrderId">The identifier of the order to cancel.</param>
public sealed record CancelOrderCommand(Guid OrderId) : ICommand;

/// <summary>
/// Validates <see cref="CancelOrderCommand"/> instances.
/// </summary>
public sealed class CancelOrderValidator : AbstractValidator<CancelOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelOrderValidator"/> class.
    /// </summary>
    public CancelOrderValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="CancelOrderCommand"/> by cancelling the targeted <see cref="Order"/>.
/// </summary>
public sealed class CancelOrderHandler : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancelOrderHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to load the targeted order.</param>
    public CancelOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    /// <inheritdoc />
    /// <exception cref="NotFoundException">Thrown when no order with the given identifier exists.</exception>
    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.Cancel();
    }
}
