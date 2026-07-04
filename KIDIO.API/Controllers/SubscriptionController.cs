using System.Threading.Tasks;
using KIDIO.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KIDIO.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionPlanService _subscriptionPlanService;

        public SubscriptionController(ISubscriptionPlanService subscriptionPlanService)
        {
            _subscriptionPlanService = subscriptionPlanService;
        }

        [HttpGet("plans")]
        public async Task<IActionResult> GetActivePlans()
        {
            var plans = await _subscriptionPlanService.GetActivePlansAsync();
            return Ok(plans);
        }
    }
}
