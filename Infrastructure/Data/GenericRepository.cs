using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {
        private readonly StoreContext _context;

        public GenericRepository(StoreContext context)
        {
            this._context = context;
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await this._context
                .Set<T>()
                .FindAsync(id);
        }

        public async Task<IReadOnlyList<T>> ListAllAsync()
        {
            return await this._context
                .Set<T>()
                .ToListAsync();
        }
    }
}