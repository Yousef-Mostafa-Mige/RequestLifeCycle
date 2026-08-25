using Ecommerceapi.Dtos.UserDtos;
using RequestLifeCycle.Dtos.Token;
using RequestLifeCycle.Dtos.UserDto;
namespace RequestLifeCycle.services
{
    public interface IUser
    {
       public Task<UserResponseDto> register (RegisterRequestDto request) ; 
       public Task<ResponceToken> login (LoginRequestDto request) ; 
    }
}