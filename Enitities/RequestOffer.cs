namespace RequestLifeCycle.Enitities
{
    public class RequestOffer
    {
        public int id { get; set; }
        public int ServiceRequestId { get; set; }
        public ServiceRequest ServiceRequest {get ;set;} = null!;
        public int RepairShopId { get; set; }
        public RepairShop RepairShop {get ;set;}= null!;
        public int OfferedPrice { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}