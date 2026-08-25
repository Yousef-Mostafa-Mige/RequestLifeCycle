using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Ecommerceapi.Dtos.UserDtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using RequestLifeCycle.data;
using RequestLifeCycle.Dtos.Token;
using RequestLifeCycle.Dtos.UserDto;
using RequestLifeCycle.Enitities;
using RequestLifeCycle.Middleware;

namespace RequestLifeCycle.services.user
{
    public class Userservices(AppDbContext context, IConfiguration configuration) : IUser
    {

        public async Task<UserResponseDto> register(RegisterRequestDto request)

        {
            if (request.Role == Enums.UserType.Admin)
            {
                throw new BadRequestException("you can't make a Admin account");
            }
            var user = await context.users.AnyAsync(p => p.Email == request.Email);
            if (user)
            {
                throw new BadRequestException("you email are exsist ");
            }
            var newuser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };
            newuser.HashPassWord = new PasswordHasher<User>().HashPassword(newuser, request.Password);
            if (request.Role == Enums.UserType.Customer)
            {
                context.users.Add(newuser);
                await context.SaveChangesAsync();
                return new UserResponseDto
                {
                    Username = request.Name,
                    CreatedAt = DateTime.UtcNow
                };
            }
            if (request.Role == Enums.UserType.shop)
            {
                if (string.IsNullOrWhiteSpace(request.ShopName) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Address))
                {
                    throw new BadRequestException(
                        "Shop information is required");
                }
                var shop = new RepairShop
                {
                    User = newuser,
                    Address = request.Address,
                    ShopName = request.ShopName,
                    Description = request.Description,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                };
                context.RepairShops.Add(shop);
                await context.SaveChangesAsync();
                return new UserResponseDto
                {
                    Username = request.Name,
                    CreatedAt = DateTime.UtcNow
                };
            }
            throw new BadRequestException("Invalid user role");
        }
        public async Task<ResponceToken> login(LoginRequestDto request)
        {
            var user = await context.users.FirstOrDefaultAsync(p => p.Email == request.Email);
            if (user is null)
            {
                throw new NotFoundException("your Email not found");
            }
            var pass = new PasswordHasher<User>().VerifyHashedPassword(user, user.HashPassWord, request.Password);
            if (pass ==
                PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }
            return await createtoken(user);
        }
        private async Task<ResponceToken> createtoken(User user)
        {
            return new ResponceToken
            {
                Acctoken = await ginrateaccesetoken(user),
                refleshtoken = await GenerateAndSaveRefreshTokenAsync(user)
                
            };
        }

        private async Task<string> ginrateaccesetoken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier,user.id.ToString()),
                new Claim(ClaimTypes.Name,user.Name),
                new Claim(ClaimTypes.Role,user.Role.ToString()),
            };
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["AppSettings:SecretKey"]!)
            );
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private string GenerateToken()
        {
            var rondomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(rondomNumber);
            return Convert.ToBase64String(rondomNumber);
        }
        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            var refreshToken = GenerateToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }
        public async Task<ResponceToken?> RefreshTokenAsync(string refreshToken)
        {
            var user = await context.users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedException("Invalid or expired refresh token.");
            }


            return await createtoken(user);
        }
    }
}