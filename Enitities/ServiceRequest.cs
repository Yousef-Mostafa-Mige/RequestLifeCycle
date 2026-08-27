using RequestLifeCycle.Enitities;
using RequestLifeCycle.Enums;

namespace RequestLifeCycle.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; } // تعديل id إلى Id
        public int CustomerId { get; set; }
        public User Customer { get; set; } = null!;
        
        public decimal ProposedPrice { get; set; } // تغيير النوع إلى decimal
        public required string Description { get; set; }
        
        // إضافة حالة الطلب لضبط الـ Lifecycle
        public RequestStatus Status { get; set; } = RequestStatus.Pending;

        public ICollection<RequestOffer> RequestOffers { get; set; } = new List<RequestOffer>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}