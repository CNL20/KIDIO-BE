using System.Collections.Generic;
using System.Threading.Tasks;
using KIDIO.Business.DTOs.Subscription;

namespace KIDIO.Business.Interfaces
{
    public interface ISubscriptionPlanService
    {
        Task<IEnumerable<SubscriptionPlanDTO>> GetActivePlansAsync();
    }
}
