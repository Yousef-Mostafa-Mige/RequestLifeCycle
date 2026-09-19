using RequestLifeCycle.DTOs.ServiceRequest;

namespace RequestLifeCycle.services
{
    public interface IServiceRequestService
    {
        Task<ServiceRequestResponseDto> CreateRequestAsync(int customerId, CreateServiceRequestDto dto);
        Task<List<ServiceRequestResponseDto>> GetMyRequestsAsync(int customerId);
        Task<ServiceRequestResponseDto> GetRequestByIdAsync(int requestId, int currentUserId, string userRole);
        Task CancelRequestAsync(int requestId, int customerId);
    }
}