using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RequestLifeCycle.Dtos.UserDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RequestLifeCycle.data;
using RequestLifeCycle.Dtos.Token;
using RequestLifeCycle.Entities;
using RequestLifeCycle.Enums;
using RequestLifeCycle.Middleware;
using RequestLifeCycle.Enitities;

namespace RequestLifeCycle.services.user
{
    public class Userservices(AppDbContext context, IConfiguration configuration) : IUser
    {
        public async Task<UserResponseDto> register(RegisterRequestDto request)
        {
            if (request.Role == UserType.Admin)
            {
                throw new BadRequestException("You cannot create an Admin account.");
            }

            var userExists = await context.users.AnyAsync(p => p.Email == request.Email);
            if (userExists)
            {
                throw new BadRequestException("This email already exists.");
            }

            var newUser = new User
            {
                Name = request.Name,
                Email = request.Email,
                Role = request.Role
            };

            newUser.HashPassWord = new PasswordHasher<User>().HashPassword(newUser, request.Password);

            if (request.Role == UserType.Customer)
            {
                context.users.Add(newUser);
            }
            else if (request.Role == UserType.shop)
            {
                if (string.IsNullOrWhiteSpace(request.ShopName) ||
                    string.IsNullOrWhiteSpace(request.Description) ||
                    string.IsNullOrWhiteSpace(request.Address))
                {
                    throw new BadRequestException("Shop information is required.");
                }

                var shop = new RepairShop
                {
                    User = newUser,
                    Address = request.Address,
                    ShopName = request.ShopName,
                    Description = request.Description,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                };

                context.RepairShops.Add(shop);
            }
            else
            {
                throw new BadRequestException("Invalid user role.");
            }

            await context.SaveChangesAsync();

            return new UserResponseDto
            {
                Username = request.Name,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<ResponceToken> login(LoginRequestDto request)
        {
            var user = await context.users.FirstOrDefaultAsync(p => p.Email == request.Email);
            if (user is null)
            {
                throw new NotFoundException("Email not found.");
            }

            var pass = new PasswordHasher<User>().VerifyHashedPassword(user, user.HashPassWord, request.Password);
            if (pass == PasswordVerificationResult.Failed)
            {
                throw new UnauthorizedException("Invalid credentials.");
            }

            return await CreateTokenAsync(user);
        }

        private async Task<ResponceToken> CreateTokenAsync(User user)
        {
            return new ResponceToken
            {
                Acctoken = await GenerateAccessTokenAsync(user),
                refleshtoken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private Task<string> GenerateAccessTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration["AppSettings:SecretKey"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), // يفضل تقليل مدة الـ AccessToken إلى ساعتين أو أسبوع كحد أقصى
                signingCredentials: creds
            );

            return Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        private static string GenerateToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
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

            return await CreateTokenAsync(user);
        }
    }
}