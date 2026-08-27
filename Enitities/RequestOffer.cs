using RequestLifeCycle.Enitities;
using RequestLifeCycle.Enums;

namespace RequestLifeCycle.Entities
{
    public class RequestOffer
    {
        public int Id { get; set; } // تعديل id إلى Id
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest { get; set; } = null!;

        public int RepairShopId { get; set; }
        public RepairShop RepairShop { get; set; } = null!;

        public decimal OfferedPrice { get; set; } // تغيير النوع إلى decimal
        public string? Description { get; set; } // إضافة وصف للعرض المقدم من المحل
        
        // إضافة حالة العرض (Pending, Accepted, Rejected)
        public OfferStatus Status { get; set; } = OfferStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}