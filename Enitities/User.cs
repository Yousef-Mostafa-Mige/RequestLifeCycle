using RequestLifeCycle.Entities;
using RequestLifeCycle.Enums;

namespace RequestLifeCycle.Enitities
{
    public class User
    {
        public int id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HashPassWord { get; set; } = string.Empty;
        public UserType Role { get; set; }
        public int RepairShopid { get; set; }
        public RepairShop? RepairShop { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public ICollection<ServiceRequest> servicerequest { get; set; } = new List<ServiceRequest>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}