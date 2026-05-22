using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KIDIO.Data.Entities;

namespace KIDIO.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly KidioDbContext _context;

        public IRepository<User> Users { get; }
        public IRepository<Child> Children { get; }
        public IRepository<Topic> Topics { get; }
        public IRepository<Lesson> Lessons { get; }
        public IRepository<LessonProgress> LessonProgresses { get; }
        public IRepository<Achievement> Achievements { get; }
        public IRepository<AchievementDefinition> AchievementDefinitions { get; }
        public IRepository<Vocabulary> Vocabularies { get; }
        public IRepository<PronunciationLog> PronunciationLogs { get; }

        public UnitOfWork(KidioDbContext context)
        {
            _context = context;
            Users = new Repository<User>(context);
            Children = new Repository<Child>(context);
            Topics = new Repository<Topic>(context);
            Lessons = new Repository<Lesson>(context);
            LessonProgresses = new Repository<LessonProgress>(context);
            Achievements = new Repository<Achievement>(context);
            AchievementDefinitions = new Repository<AchievementDefinition>(context);
            Vocabularies = new Repository<Vocabulary>(context);
            PronunciationLogs = new Repository<PronunciationLog>(context);
        }

        public Task<int> SaveChangesAsync(CancellationToken ct = default)
            => _context.SaveChangesAsync(ct);

        public void Dispose() => _context.Dispose();
    }
}
