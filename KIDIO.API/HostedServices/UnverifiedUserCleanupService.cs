using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KIDIO.Common.Enums;

namespace KIDIO.API.HostedServices
{
    public class UnverifiedUserCleanupService : BackgroundService
    {
        private readonly ILogger<UnverifiedUserCleanupService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _deleteAfterDays;
        private readonly int _runAtHour;

        public UnverifiedUserCleanupService(
            ILogger<UnverifiedUserCleanupService> logger,
            IServiceScopeFactory scopeFactory,
            IConfiguration configuration)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _deleteAfterDays = configuration.GetValue<int>("CleanupSettings:DeleteAfterDays", 7);
            _runAtHour = configuration.GetValue<int>("CleanupSettings:RunAtHour", 3);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UnverifiedUserCleanupService is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, _runAtHour, 0, 0);

                if (now >= nextRun)
                {
                    nextRun = nextRun.AddDays(1);
                }

                var delay = nextRun - now;
                _logger.LogInformation($"Next user cleanup scheduled in {delay.TotalHours:F2} hours (at {nextRun}).");

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                try
                {
                    await PerformCleanupAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing user cleanup.");
                }
            }
        }

        private async Task PerformCleanupAsync(CancellationToken ct)
        {
            _logger.LogInformation("Cleanup started...");

            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<KidioDbContext>();

            // Tính mốc thời gian để đưa vào "Thùng rác" (Soft Delete)
            var softDeleteCutoffDate = DateTime.UtcNow.AddDays(-_deleteAfterDays);

            // Tính mốc thời gian để "Xóa vĩnh viễn" (Hard Delete) - Đã bị Soft Delete quá 7 ngày
            var hardDeleteCutoffDate = DateTime.UtcNow.AddDays(-7); 
            
            // 1. HARD DELETE: Xóa triệt để các user đã bị Soft Delete quá 7 ngày
            var usersToHardDelete = await dbContext.Users
                .IgnoreQueryFilters()
                .Where(u => u.IsDeleted 
                            && !u.IsEmailConfirmed 
                            && u.RefreshToken == null 
                            && u.Role != UserRole.Admin 
                            && u.UpdatedAt != null 
                            && u.UpdatedAt < hardDeleteCutoffDate)
                .ToListAsync(ct);

            if (usersToHardDelete.Any())
            {
                dbContext.Users.RemoveRange(usersToHardDelete);
                await dbContext.SaveChangesAsync(ct);
                _logger.LogInformation($"Hard deleted {usersToHardDelete.Count} expired users.");
            }

            // 2. SOFT DELETE: Tìm các user ảo/chưa verify quá số ngày quy định
            var usersToSoftDelete = await dbContext.Users
                .Where(u => !u.IsDeleted 
                            && !u.IsEmailConfirmed 
                            && u.RefreshToken == null 
                            && u.Role != UserRole.Admin 
                            && u.CreatedAt < softDeleteCutoffDate)
                .ToListAsync(ct);

            if (usersToSoftDelete.Any())
            {
                foreach (var u in usersToSoftDelete)
                {
                    u.IsDeleted = true;
                    u.UpdatedAt = DateTime.UtcNow;
                }
                await dbContext.SaveChangesAsync(ct);
                _logger.LogInformation($"Found and soft-deleted {usersToSoftDelete.Count} unverified users.");
            }
            else
            {
                _logger.LogInformation("No expired users found for soft-delete.");
            }

            _logger.LogInformation("Cleanup finished.");
        }
    }
}
