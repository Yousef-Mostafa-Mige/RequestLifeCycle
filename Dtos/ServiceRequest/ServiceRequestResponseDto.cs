using RequestLifeCycle.Enums;

namespace RequestLifeCycle.DTOs.ServiceRequest
{
    public class ServiceRequestResponseDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ProposedPrice { get; set; }
        public RequestStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
        public int OffersCount { get; set; }
    }
}