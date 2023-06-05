using System.Net;
using API.Dtos;
using API.Errors;
using API.Helpers;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
	public class ProductsController : BaseApiController
	{
		private readonly IGenericRepository<Product> _productsRepo;
		private readonly IGenericRepository<ProductBrand> _productBrandRepo;
		private readonly IGenericRepository<ProductType> _productTypeRepo;
		private readonly IMapper _mapper;

		public ProductsController(
			IGenericRepository<Product> productsRepo,
			IGenericRepository<ProductBrand> productBrandRepo,
			IGenericRepository<ProductType> productTypeRepo,
			IMapper mapper)
		{
			this._productsRepo = productsRepo;
			this._productBrandRepo = productBrandRepo;
			this._productTypeRepo = productTypeRepo;
			this._mapper = mapper;
		}

		[Cached(600)]
		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts(
			[FromQuery]ProductSpecParams productParams)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(productParams);

			var countSpec = new ProductWithFiltersForCountSpecification(productParams);
			
			var totalItems = await this._productsRepo.CountAsync(countSpec);
			
			var products = await this._productsRepo.ListAsync(spec);
			
			var data = this._mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

			return Ok(new Pagination<ProductToReturnDto>()
			{
				PageIndex = productParams.PageIndex,
				PageSize = productParams.PageSize,
				Count = totalItems,
				Data = data
			});
		}

		[Cached(600)]
		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
		public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
		{
			var spec = new ProductsWithTypesAndBrandsSpecification(id);

			var product = await this._productsRepo.GetEntityWithSpec(spec);

			if (product == null)
			{
				return NotFound(new ApiResponse((int)HttpStatusCode.NotFound));
			}

			return this._mapper.Map<Product, ProductToReturnDto>(product);
		}

		[Cached(600)]
		[HttpGet("brands")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductBrands()
		{
			return Ok(await this._productBrandRepo.ListAllAsync());
		}

		[HttpGet("types")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetProductTypes()
		{
			return Ok(await this._productTypeRepo.ListAllAsync());
		}
	}
}