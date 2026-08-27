using RequestLifeCycle.Enums;

namespace RequestLifeCycle.DTOs.RequestOffer
{
    public class OfferResponseDto
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public int RepairShopId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public decimal OfferedPrice { get; set; }
        public string? Description { get; set; }
        public OfferStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreatedAt { get; set; }
    }
}