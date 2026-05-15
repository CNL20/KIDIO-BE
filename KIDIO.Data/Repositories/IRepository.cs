using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KIDIO.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
        Task AddAsync(T entity, CancellationToken ct = default);
        void Update(T entity);
        void Remove(T entity);
        IQueryable<T> Query();
    }
}
//Expression chính là biểu thức Lambda (khác delegate): cho phép truyền “logic truy vấn” xuống database bằng LINQ, EF Core, v.v.
//Ví dụ:
//predicate: x => x.Age > 18(lọc các user lớn hơn 18 tuổi)
//predicate: x => x.Status == Status.Active(lọc những entity có status active)

//CancellationToken là một cơ chế của .NET cho phép hủy bỏ hành động không đồng bộ (async) hoặc lệnh dài giữa chừng, chủ động, khi không còn cần thiết.
//Ví dụ: Người dùng chuyển trang web hoặc thoát giữa chừng, bạn muốn dừng mọi truy vấn database/apicall để tiết kiệm tài nguyên.