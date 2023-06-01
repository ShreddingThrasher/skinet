using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Entities;
using Core.Interfaces;

namespace Infrastructure.Data
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly StoreContext _context;
		private Hashtable _repositories;
		
		public UnitOfWork(StoreContext context)
		{
			this._context = context;
		}
		
		public async Task<int> Complete()
		{
			return await this._context.SaveChangesAsync();
		}

		public void Dispose()
		{
			this._context.Dispose();
		}

		public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : BaseEntity
		{
			if(this._repositories == null) this._repositories = new Hashtable();
			
			var type = typeof(TEntity).Name;
			
			if(!this._repositories.ContainsKey(type))
			{
				var repositoryType = typeof(GenericRepository<>);
				var repositoryInstance = Activator.CreateInstance(
					repositoryType.MakeGenericType(typeof(TEntity)), this._context);
					
				this._repositories.Add(type, repositoryInstance);
			}
			
			return (IGenericRepository<TEntity>)this._repositories[type];
		}
	}
}