using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.OrderAggregate;

namespace API.Helpers
{
	public class MappingProfiles : Profile
	{
		public MappingProfiles()
		{
			CreateMap<Product, ProductToReturnDto>()
				.ForMember(dto => dto.ProductBrand, options => 
					options.MapFrom(p => p.ProductBrand.Name))
				.ForMember(dto => dto.ProductType, options =>
					options.MapFrom(p => p.ProductType.Name))
				.ForMember(dto => dto.PictureUrl, options =>
					options.MapFrom<ProductUrlResolver>());
					
			CreateMap<Core.Entities.Identity.Address, AddressDto>().ReverseMap();
			CreateMap<CustomerBasketDto, CustomerBasket>();
			CreateMap<BasketItemDto, BasketItem>();
			CreateMap<AddressDto, Core.Entities.OrderAggregate.Address>();
			CreateMap<Order, OrderToReturnDto>()
				.ForMember(d => d.DeliveryMethod, o => o.MapFrom(s => s.DeliveryMethod.ShortName))
				.ForMember(d => d.ShippingPrice, o => o.MapFrom(s => s.DeliveryMethod.Price));
			CreateMap<OrderItem, OrderItemDto>()
				.ForMember(d => d.ProductId, o => o.MapFrom(s => s.ItemOrdered.ProductItemId))
				.ForMember(d => d.ProductName, o => o.MapFrom(s => s.ItemOrdered.ProductName))
				.ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.ItemOrdered.PictureUrl))
				.ForMember(d => d.PictureUrl, o => o.MapFrom<OrderItemUrlResolver>());
		}
	}
}