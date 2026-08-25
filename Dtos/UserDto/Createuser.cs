using RequestLifeCycle.Enums;
namespace RequestLifeCycle.Dtos.UserDto
{
    
public class RegisterRequestDto
{
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }

    public  UserType Role { get; set; }

    // Shop Data
    public string? ShopName { get; set; }
    public string? Description { get; set; }
    public string? Address { get; set; }
}
}