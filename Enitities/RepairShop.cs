using RequestLifeCycle.Enitities;

namespace RequestLifeCycle.Entities
{
    public class RepairShop
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public required string ShopName { get; set; }
        public required string Description { get; set; }
        public required string Address { get; set; }
        public bool IsAvailable { get; set; } = true; // إزالة كلمة required مع البولين

        public ICollection<RequestOffer> RequestOffers { get; set; } = new List<RequestOffer>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}