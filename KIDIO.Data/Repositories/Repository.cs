using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly KidioDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(KidioDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }
        public async Task AddAsync(T entity, CancellationToken ct = default)
            => await _dbSet.AddAsync(entity, ct);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.Where(predicate).ToListAsync(ct);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
            => await _dbSet.FirstOrDefaultAsync(predicate, ct);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
            => await _dbSet.ToListAsync(ct);

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _dbSet.FindAsync(new object[] { id }, ct);

        public IQueryable<T> Query()
            => _dbSet.AsQueryable();

        public void Remove(T entity)
             => _dbSet.Remove(entity);

        public void Update(T entity)
            => _dbSet.Update(entity);
    }
}
