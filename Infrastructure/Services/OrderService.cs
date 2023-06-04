using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;

namespace Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepo, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this._basketRepo = basketRepo;
        }

        public async Task<Order> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            // get basket from the repo
            var basket = await this._basketRepo.GetBasketAsync(basketId);

            // get items from the product repo
            var items = new List<OrderItem>();
            foreach (var item in basket.Items)
            {
                var productItem = await this._unitOfWork.Repository<Product>().GetByIdAsync(item.Id);
                var itemOrdered = new ProductItemOrdered(productItem.Id,
                    productItem.Name, productItem.PictureUrl);
                var orderItem = new OrderItem(itemOrdered, productItem.Price, item.Quantity);
                items.Add(orderItem);
            }

            // get delivery method from repo
            var deliveryMethod = await this._unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);

            // calculate subtotal
            var subtotal = items.Sum(i => i.Price);

            // check to see if order exists
            var spec = new OrderByPaymentIntentIdSpecification(basket.PaymentIntentId);
            var order = await this._unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

            if (order != null)
            {
                order.ShipToAddress = shippingAddress;
                order.DeliveryMethod = deliveryMethod;
                order.Subtotal = subtotal;
                this._unitOfWork.Repository<Order>().Update(order);
            }
            else
            {
                // create order
                order = new Order(items, buyerEmail, shippingAddress,
                    deliveryMethod, subtotal, basket.PaymentIntentId);
                this._unitOfWork.Repository<Order>().Add(order);
            }

            // save to db
            var result = await this._unitOfWork.Complete();

            // if nothing saved to database
            if (result <= 0) return null;

            // return the order
            return order;
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            return await this._unitOfWork.Repository<DeliveryMethod>().ListAllAsync();
        }

        public async Task<Order> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);

            return await this._unitOfWork.Repository<Order>().GetEntityWithSpec(spec);
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);

            return await this._unitOfWork.Repository<Order>().ListAsync(spec);
        }
    }
}