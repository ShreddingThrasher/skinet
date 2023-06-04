using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IBasketRepository _basketRepository;
		private readonly IUnitOfWork _unitOfWork;
		private IConfiguration _config;
		
		public PaymentService(IBasketRepository basketRepository, IUnitOfWork unitOfWork,
			IConfiguration config)
		{
			this._basketRepository = basketRepository;
			this._unitOfWork = unitOfWork;
			this._config = config;
		}

		public async Task<CustomerBasket> CreateOrUpdatePaymentIntent(string basketId)
		{
			StripeConfiguration.ApiKey = this._config["StripeSettings:SecretKey"];
			
			var basket = await this._basketRepository.GetBasketAsync(basketId);

			if (basket == null) return null;
			
			var shippingPrice = 0m;
			
			if (basket.DeliveryMethodId.HasValue)
			{
				var deliveryMethod = await this._unitOfWork.Repository<DeliveryMethod>()
					.GetByIdAsync((int)basket.DeliveryMethodId);
				shippingPrice = deliveryMethod.Price;
			}
			
			foreach (var item in basket.Items)
			{
				var productItem = await this._unitOfWork.Repository<Core.Entities.Product>()
					.GetByIdAsync(item.Id);
					
				if (item.Price != productItem.Price)
				{
					item.Price = productItem.Price;
				}
			}
			
			var service = new PaymentIntentService();
			PaymentIntent intent;
			
			if (string.IsNullOrEmpty(basket.PaymentIntentId))
			{
				var options = new PaymentIntentCreateOptions
				{
					Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100))
						+ (long)shippingPrice,
					Currency = "usd",
					PaymentMethodTypes = new List<string>()
					{
						"card"
					}
				};
				
				intent = await service.CreateAsync(options);
				basket.PaymentIntentId = intent.Id;
				basket.ClientSecret = intent.ClientSecret;
			}
			else
			{
				var options = new PaymentIntentUpdateOptions
				{
					Amount = (long)basket.Items.Sum(i => i.Quantity * (i.Price * 100))
						+ (long)shippingPrice * 100
				};
				
				await service.UpdateAsync(basket.PaymentIntentId, options);
			}
			
			await this._basketRepository.UpdateBasketAsync(basket);
			
			return basket;
		}

		public async Task<Order> UpdateOrderPaymentFailed(string paymentIntentId)
		{
			var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
			var order = await this._unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

			if (order == null) return null;
			
			order.Status = OrderStatus.PaymentFailed;
			await this._unitOfWork.Complete();
			
			return order;
		}

		public async Task<Order> UpdateOrderPaymentSucceeded(string paymentIntentId)
		{
			var spec = new OrderByPaymentIntentIdSpecification(paymentIntentId);
			var order = await this._unitOfWork.Repository<Order>().GetEntityWithSpec(spec);

			if (order == null) return null;
			
			order.Status = OrderStatus.PaymentReceived;
			await this._unitOfWork.Complete();
			
			return order;
		}
	}
}