using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.Subscription;
using KIDIO.Business.Interfaces;
using KIDIO.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services.Subscription
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly KidioDbContext _context;

        public SubscriptionPlanService(KidioDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SubscriptionPlanDTO>> GetActivePlansAsync()
        {
            return await _context.SubscriptionPlans
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new SubscriptionPlanDTO
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description,
                    Price = x.Price,
                    DurationInDays = x.DurationInDays,
                    DisplayOrder = x.DisplayOrder
                })
                .ToListAsync();
        }
    }
}
