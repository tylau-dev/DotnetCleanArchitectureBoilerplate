using Application.Common.Exceptions;
using Application.Common.Messaging;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Commands;

/// <summary>
/// Command that adds a line item to an order in <see cref="Domain.Orders.OrderStatus.Draft"/> status.
/// </summary>
/// <param name="OrderId">The identifier of the order to add the item to.</param>
/// <param name="ProductId">The identifier of the product being added.</param>
/// <param name="ProductName">A snapshot of the product's name at the time it is added.</param>
/// <param name="UnitPrice">The unit price of the product.</param>
/// <param name="Currency">The ISO 4217 currency code of <paramref name="UnitPrice"/>.</param>
/// <param name="Quantity">The quantity of the product being added. Must be greater than zero.</param>
public sealed record AddOrderItemCommand(
    Guid OrderId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    string Currency,
    int Quantity) : ICommand;

/// <summary>
/// Validates <see cref="AddOrderItemCommand"/> instances.
/// </summary>
public sealed class AddOrderItemValidator : AbstractValidator<AddOrderItemCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AddOrderItemValidator"/> class.
    /// </summary>
    public AddOrderItemValidator()
    {
        RuleFor(command => command.OrderId).NotEmpty();
        RuleFor(command => command.ProductId).NotEmpty();
        RuleFor(command => command.ProductName).NotEmpty();
        RuleFor(command => command.UnitPrice).GreaterThanOrEqualTo(0m);
        RuleFor(command => command.Currency).NotEmpty();
        RuleFor(command => command.Quantity).GreaterThan(0);
    }
}

/// <summary>
/// Handles <see cref="AddOrderItemCommand"/> by adding a line item to the targeted <see cref="Order"/>.
/// </summary>
public sealed class AddOrderItemHandler : IRequestHandler<AddOrderItemCommand>
{
    private readonly IOrderRepository orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="AddOrderItemHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to load the targeted order.</param>
    public AddOrderItemHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    /// <inheritdoc />
    /// <exception cref="NotFoundException">Thrown when no order with the given identifier exists.</exception>
    public async Task Handle(AddOrderItemCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(new OrderId(request.OrderId), cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException(nameof(Order), request.OrderId);

        order.AddItem(
            new ProductId(request.ProductId),
            request.ProductName,
            new Money(request.UnitPrice, request.Currency),
            request.Quantity);
    }
}
