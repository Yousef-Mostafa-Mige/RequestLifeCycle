using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RequestLifeCycle.DTOs.RequestOffer;
using RequestLifeCycle.services;

namespace RequestLifeCycle.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequestOffersController : ControllerBase
    {
        private readonly IRequestOfferService _offerService;

        public RequestOffersController(IRequestOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpPost]
        [Authorize(Roles = "RepairShop")]
        public async Task<IActionResult> Create(CreateOfferDto dto)
        {
            int userId = GetUserIdFromClaims();
            var result = await _offerService.CreateOfferAsync(userId, dto);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpGet("request/{requestId}")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetOffersForRequest(int requestId)
        {
            int customerId = GetUserIdFromClaims();
            var result = await _offerService.GetOffersForRequestAsync(requestId, customerId);
            return Ok(result);
        }

        [HttpPut("{offerId}/accept")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Accept(int offerId)
        {
            int customerId = GetUserIdFromClaims();
            await _offerService.AcceptOfferAsync(offerId, customerId);
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