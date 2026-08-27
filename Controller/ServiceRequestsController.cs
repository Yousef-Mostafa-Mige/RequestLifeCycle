using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestLifeCycle.DTOs.ServiceRequest;
using RequestLifeCycle.services;

namespace RequestLifeCycle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _requestService;

        public ServiceRequestsController(IServiceRequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(CreateServiceRequestDto dto)
        {
            int customerId = GetUserIdFromClaims();
            var result = await _requestService.CreateRequestAsync(customerId, dto);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("my-requests")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetMyRequests()
        {
            int customerId = GetUserIdFromClaims();
            var result = await _requestService.GetMyRequestsAsync(customerId);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            int userId = GetUserIdFromClaims();
            string role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

            var result = await _requestService.GetRequestByIdAsync(id, userId, role);
            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Cancel(int id)
        {
            int customerId = GetUserIdFromClaims();
            await _requestService.CancelRequestAsync(id, customerId);
            return NoContent();
        }

        private int GetUserIdFromClaims()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? User.FindFirst("sub")?.Value;

            if (int.TryParse(userIdClaim, out int userId))
                return userId;

            throw new UnauthorizedAccessException("معرف المستخدم غير صالح في الـ JWT.");
        }
    }
}