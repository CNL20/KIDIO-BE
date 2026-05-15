using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Data.Entities;

namespace KIDIO.Data.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<Child> Children { get; }
        IRepository<Topic> Topics { get; }
        IRepository<Lesson> Lessons { get; }
        IRepository<LessonProgress> LessonProgresses { get; }
        IRepository<Achievement> Achievements { get; }
        IRepository<Vocabulary> Vocabularies { get; }
        IRepository<PronunciationLog> PronunciationLogs { get; }

        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
