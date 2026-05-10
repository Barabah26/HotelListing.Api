using HotelListing.Api.Application.DTOs.Auth;
using HotelListing.Api.Common.Results;

namespace HotelListing.Api.Application.Contracts
{
    public interface IUserService
    {
        string UserId { get; }
        Task<Result<string>> LoginAsync(LoginDto dto);
        Task<Result<RegisteredUserDto>> RegisterAsync(RegisterUserDto registerUserDto);
    }
}