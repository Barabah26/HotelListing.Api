namespace HotelListing.Api.Contracts;

public interface IApiKeyValidatorService
{
    Task<bool> IsValidAsync(string key, CancellationToken ct = default);
}