using ECommerecAPI.DTOs;
using ECommerecAPI.Models;
using System.Security.Claims;

namespace ECommerecAPI.Services
{
    public class OrderProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderProductService> _logger;

        public OrderProductService(
            ILogger<OrderProductService> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public object PlaceOrder(PlaceOrderDTO request, ClaimsPrincipal currentUser)
        {
            _logger.LogInformation("PlaceOrder called with {ItemCount} items", request.Items.Count);

            var userIdClaim = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
            {
                _logger.LogWarning("PlaceOrder failed - invalid token, no user ID claim");
                return new { statusCode = 401, message = "Invalid token." };
            }

            int userId = int.Parse(userIdClaim);
            _logger.LogInformation("PlaceOrder requested by User ID: {UserId}", userId);

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                _logger.LogWarning("PlaceOrder failed - user not found: {UserId}", userId);
                return new { statusCode = 404, message = "User not found." };
            }

            var resolvedItems = new List<(OrderProducts orderProduct, Product product, int quantity)>();
            decimal totalAmount = 0;

            foreach (var item in request.Items)
            {
                var product = _context.Products.Find(item.Pid);
                if (product == null)
                {
                    _logger.LogWarning("PlaceOrder failed - product not found: {ProductId}", item.Pid);
                    return new { statusCode = 404, message = $"Product with ID {item.Pid} does not exist." };
                }

                if (product.Stock < item.qnt)
                {
                    _logger.LogWarning(
                        "PlaceOrder failed - insufficient stock for product: {ProductName}. " +
                        "Available: {Available}, Requested: {Requested}",
                        product.Name, product.Stock, item.qnt);

                    return new
                    {
                        statusCode = 400,
                        message = $"Insufficient stock for product '{product.Name}'. " +
                                  $"Available: {product.Stock}, Requested: {item.qnt}."
                    };
                }

                totalAmount += product.Price * item.qnt;

                _logger.LogInformation(
                    "Product validated: {ProductName} | Qty: {Qty} | Price: {Price}",
                    product.Name, item.qnt, product.Price);

                resolvedItems.Add((
                    new OrderProducts
                    {
                        ProductId = item.Pid,
                        Quantity = item.qnt,
                        Price = product.Price
                    },
                    product,
                    item.qnt
                ));
            }

            var order = new Order
            {
                UserId = userId,
                orderDate = DateTime.Now
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            _logger.LogInformation("Order created with ID: {OrderId} for User: {UserId}", order.order_Id, userId);

            var orderProducts = new List<OrderProducts>();
            foreach (var (orderProduct, product, quantity) in resolvedItems)
            {
                orderProduct.OrderId = order.order_Id;
                orderProducts.Add(orderProduct);
                product.Stock -= quantity;

                _logger.LogInformation(
                    "Stock updated for product: {ProductName} | New stock: {Stock}",
                    product.Name, product.Stock);
            }

            _context.orderProducts.AddRange(orderProducts);
            _context.SaveChanges();

            _logger.LogInformation(
                "Order placed successfully. OrderID: {OrderId} | Total: {Total} | Items: {Count}",
                order.order_Id, totalAmount, orderProducts.Count);

            return new
            {
                statusCode = 200,
                data = new
                {
                    message = "Order placed successfully.",
                    orderId = order.order_Id,
                    totalAmount,
                    itemsOrdered = orderProducts.Count
                }
            };
        }
    }
}