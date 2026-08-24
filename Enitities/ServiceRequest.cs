
using System.Collections.ObjectModel;

namespace RequestLifeCycle.Enitities
{
    public class ServiceRequest
    {
        public int id { get; set; }
        public int CustomerId { get; set; }
        public User Customer  {get;set;} = null!;
        public int ProposedPrice { get; set; }
        public required string Description { get; set; } = string.Empty;
        public ICollection<RequestOffer> requestOffers { get; set; } = new List<RequestOffer>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}