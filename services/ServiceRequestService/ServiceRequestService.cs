using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.data;
using RequestLifeCycle.DTOs.ServiceRequest;
using RequestLifeCycle.Entities;
using RequestLifeCycle.Enums;
using services.CashingServices;

namespace RequestLifeCycle.services
{
    public class ServiceRequestService(AppDbContext _context, ICaching cachingService) : IServiceRequestService
    {


        public async Task<ServiceRequestResponseDto> CreateRequestAsync(int customerId, CreateServiceRequestDto dto)
        {
            var request = new ServiceRequest
            {
                CustomerId = customerId,
                Description = dto.Description,
                ProposedPrice = dto.ProposedPrice,
                Status = RequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            await cachingService.RemoveAsync($"my_requests_{customerId}");
            Console.WriteLine($"Generated ID: {request.id}"); // Debugging line to check the generated ID
            return new ServiceRequestResponseDto
            {
                Id = request.id,
                CustomerId = request.CustomerId,
                Description = request.Description,
                ProposedPrice = request.ProposedPrice,
                Status = request.Status,
                CreatedAt = request.CreatedAt,
                OffersCount = 0
            };
        }

        public async Task<List<ServiceRequestResponseDto>> GetMyRequestsAsync(int customerId)
        {
            var cacheKey = $"my_requests_{customerId}";
            var requests = await cachingService.GetOrCreateAsync(cacheKey, fetchFunction: () => FetchRequestsFromDbAsync(customerId));
            return requests;
        }
        private Task<List<ServiceRequestResponseDto>> FetchRequestsFromDbAsync(int customerId)
        {
            return _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ServiceRequestResponseDto
                {
                    Id = r.id,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    Description = r.Description,
                    ProposedPrice = r.ProposedPrice,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    OffersCount = r.RequestOffers.Count()
                })
                .ToListAsync();
        }

        public async Task<ServiceRequestResponseDto> GetRequestByIdAsync(int requestId, int currentUserId, string userRole)
        {

            var cacheKey = $"request_by_id_{requestId}_role_{userRole}_user_{currentUserId}";
            var requests = await cachingService.GetOrCreateAsync(cacheKey, fetchFunction: () => FetchRequestsFromDbAsync(requestId, currentUserId, userRole));
            return requests;
        }

        private async Task<ServiceRequestResponseDto> FetchRequestsFromDbAsync(int requestId, int currentUserId, string userRole)
        {
            var request = await _context.ServiceRequests
                .AsNoTracking()
                .Where(r => r.id == requestId)
                .Select(r => new ServiceRequestResponseDto
                {
                    Id = r.id,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    Description = r.Description,
                    ProposedPrice = r.ProposedPrice,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt,
                    OffersCount = r.RequestOffers.Count()
                })
                .FirstOrDefaultAsync();

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            // Verification of Ownership & Access Rights
            if (userRole == "Customer" && request.CustomerId != currentUserId)
                throw new UnauthorizedAccessException("غير مصرح لك برؤية تفاصيل هذا الطلب.");

            return request;
        }

        public async Task CancelRequestAsync(int requestId, int customerId)
        {
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.id == requestId);

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            // Ownership Check
            if (request.CustomerId != customerId)
                throw new UnauthorizedAccessException("لا يمكنك إلغاء طلب لا تملكه.");

            // Business Rule State Check
            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("لا يمكن إلغاء الطلب إلا إذا كان في حالة Pending.");

            request.Status = RequestStatus.Cancelled;
            await _context.SaveChangesAsync();
            await cachingService.RemoveAsync($"my_requests_{customerId}");
            await cachingService.RemoveAsync($"request_by_id_{requestId}_role_Customer_user_{customerId}");
        } 
    }
}