using API.Errors;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
	public class PaymentsController : BaseApiController
	{
		private const string WhSecret = "whsec_479815060e1fe0a02fc9b3303b04c9d0ed2690ebfb6193cfb2caaa1bde36e08f";
		private readonly IPaymentService _paymentService;
		private readonly ILogger<PaymentsController> _logger;

		public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
		{
			this._paymentService = paymentService;
			this._logger = logger;
		}

		[Authorize]
		[HttpPost("{basketId}")]
		public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
		{
			var basket = await this._paymentService.CreateOrUpdatePaymentIntent(basketId);

			if (basket == null) return BadRequest(new ApiResponse(400, "Problem with your basket"));

			return basket;
		}

		//[AllowAnonymous]
		[HttpPost("webhook")]
		public async Task<ActionResult> StripeWebhook()
		{
			var json = await new StreamReader(Request.Body).ReadToEndAsync();

			var stripeEvent = EventUtility.ConstructEvent(json,
				Request.Headers["Stripe-Signature"], WhSecret);

			PaymentIntent intent;
			Order order;

			switch (stripeEvent.Type)
			{
				case "payment_intent.succeeded":
					intent = (PaymentIntent)stripeEvent.Data.Object;
					this._logger.LogInformation("Payment succeeded: ", intent.Id);
					order = await this._paymentService.UpdateOrderPaymentSucceeded(intent.Id);
					this._logger.LogInformation("Order updated to payment received: ", order.Id);
					break;
				case "payment_intent.payment_failed":
					intent = (PaymentIntent)stripeEvent.Data.Object;
					this._logger.LogInformation("Payment failed: ", intent.Id);
					order = await this._paymentService.UpdateOrderPaymentFailed(intent.Id);
					this._logger.LogInformation("Order updated to payment received: ", order.Id);
					break;
			}
			
			return new EmptyResult();
		}
	}
}