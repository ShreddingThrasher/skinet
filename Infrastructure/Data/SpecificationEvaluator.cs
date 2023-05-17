using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
	public class SpecificationEvaluator<TEntity> where TEntity : BaseEntity
	{
		public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery, ISpecification<TEntity> spec)
		{
			var query = inputQuery;
			
			//Filters if a criteria is present
			if (spec.Criteria != null)
			{
				query = query.Where(spec.Criteria);
			}
			
			//Orders ascending if an OrderBy is present
			if (spec.OrderBy != null)
			{
				query = query.OrderBy(spec.OrderBy);
			}

			//Orders descending if an OrderByDesc is present
			if (spec.OrderByDescending != null)
			{
				query = query.OrderByDescending(spec.OrderByDescending);
			}
			
			//If paging is enabled - skips and takes a given amount for items per page
			if(spec.IsPagingEnabled)
			{
				query = query.Skip(spec.Skip).Take(spec.Take);
			}
			
			//Aggregates the query using the include statements
			query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

			return query;
		}
	}
}