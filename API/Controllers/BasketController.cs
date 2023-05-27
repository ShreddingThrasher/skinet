using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class BasketController : BaseApiController
	{
		private readonly IBasketRepository _basketRepository;
		
		public BasketController(IBasketRepository basketRepository)
		{
			this._basketRepository = basketRepository;
		}
		
		[HttpGet]
		public async Task<ActionResult<CustomerBasket>> GetBasketById(string id)
		{
			var basket = await this._basketRepository.GetBasketAsync(id);
			
			return Ok(basket ?? new CustomerBasket(id));
		}
		
		[HttpPost]
		public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasket basket)
		{
			var updatedBasket = await this._basketRepository.UpdateBasketAsync(basket);
			
			return Ok(updatedBasket);
		}
		
		[HttpDelete]
		public async Task DeleteBasketAsync(string id)
		{
			await this._basketRepository.DeleteBasketAsync(id);
		}
	}
}