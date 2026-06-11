using Application.Common.Messaging;
using Domain.Orders;
using Domain.Orders.ValueObjects;
using FluentValidation;
using MediatR;

namespace Application.Orders.Commands;

/// <summary>
/// Command that creates a new order in <see cref="Domain.Orders.OrderStatus.Draft"/> status for
/// the given customer and shipping address.
/// </summary>
/// <param name="CustomerId">The identifier of the customer placing the order.</param>
/// <param name="Street">The street address, including house or unit number.</param>
/// <param name="City">The city or locality.</param>
/// <param name="PostalCode">The postal or ZIP code.</param>
/// <param name="Country">The country.</param>
public sealed record CreateOrderCommand(
    Guid CustomerId,
    string Street,
    string City,
    string PostalCode,
    string Country) : ICommand<Guid>;

/// <summary>
/// Validates <see cref="CreateOrderCommand"/> instances.
/// </summary>
public sealed class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderValidator"/> class.
    /// </summary>
    public CreateOrderValidator()
    {
        RuleFor(command => command.CustomerId).NotEmpty();
        RuleFor(command => command.Street).NotEmpty();
        RuleFor(command => command.City).NotEmpty();
        RuleFor(command => command.PostalCode).NotEmpty();
        RuleFor(command => command.Country).NotEmpty();
    }
}

/// <summary>
/// Handles <see cref="CreateOrderCommand"/> by creating and persisting a new <see cref="Order"/>.
/// </summary>
public sealed class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository orderRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateOrderHandler"/> class.
    /// </summary>
    /// <param name="orderRepository">The repository used to persist the new order.</param>
    public CreateOrderHandler(IOrderRepository orderRepository)
    {
        this.orderRepository = orderRepository;
    }

    /// <inheritdoc />
    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = Order.Create(
            new CustomerId(request.CustomerId),
            new Address(request.Street, request.City, request.PostalCode, request.Country));

        await orderRepository.AddAsync(order, cancellationToken).ConfigureAwait(false);

        return order.Id.Value;
    }
}
