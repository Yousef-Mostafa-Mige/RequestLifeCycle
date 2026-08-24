namespace RequestLifeCycle.Enitities
{
    public class RepairShop
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public required string ShopName { get; set; }
        public required string Description { get; set; }
        public required string Address { get; set; }
        public required bool IsAvailable { get; set; } = true;
        public ICollection<RequestOffer> requestOffers { get; set; } = new List<RequestOffer>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}