using API.Dtos;
using AutoMapper;
using Core.Entities;
using Core.Entities.Identity;

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
					
			CreateMap<Address, AddressDto>().ReverseMap();
		}
	}
}