using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Core.Entities;

namespace Core.Specifications
{
	public class ProductsWithTypesAndBrandsSpecification : BaseSpecification<Product>
	{
		public ProductsWithTypesAndBrandsSpecification(ProductSpecParams productParams)
			: base(x => 
				(string.IsNullOrEmpty(productParams.Search) || x.Name.ToLower().Contains(productParams.Search)) &&
				(!productParams.BrandId.HasValue || x.ProductBrandId == productParams.BrandId) && 
				(!productParams.TypeId.HasValue || x.ProductTypeId == productParams.TypeId))
		{
			this.AddInclude(x => x.ProductType);
			this.AddInclude(x => x.ProductBrand);
			this.AddOrderBy(x => x.Name);
			this.ApplyPaging(productParams.PageSize * (productParams.PageIndex -1), productParams.PageSize);

			if (!string.IsNullOrEmpty(productParams.Sort))
			{
				switch (productParams.Sort)
				{
					case "priceAsc":
						this.AddOrderBy(p => p.Price);
						break;
					case "priceDesc":
						this.AddOrderByDescending(p => p.Price);
						break;
					default:
						this.AddOrderBy(p => p.Name);
						break;
				}
			}
		}

		public ProductsWithTypesAndBrandsSpecification(int id)
			: base(x => x.Id == id)
		{
			this.AddInclude(x => x.ProductType);
			this.AddInclude(x => x.ProductBrand);
		}
	}
}