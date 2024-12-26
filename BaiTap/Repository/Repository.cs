using BaiTap.IRepository;
using BaiTap.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace BaiTap.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly Model1 context;
        private readonly DbSet<T> dbSet;

        public Repository(Model1 context)
        {
            this.context = context;
            this.dbSet = context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return await dbSet.FindAsync(id);
        }

        public async Task InsertAsync(T entity)
        {
            dbSet.Add(entity);
            await SaveAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            await SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            T entity = await dbSet.FindAsync(id);
            dbSet.Remove(entity);
            await SaveAsync();
        }

        public async Task SaveAsync()
        {
            await context.SaveChangesAsync();
        }
    }
}
