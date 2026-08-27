using RequestLifeCycle.DTOs.RequestOffer;

namespace RequestLifeCycle.services
{
    public interface IRequestOfferService
    {
        Task<OfferResponseDto> CreateOfferAsync(int userId, CreateOfferDto dto);
        Task<IEnumerable<OfferResponseDto>> GetOffersForRequestAsync(int requestId, int customerId);
        Task AcceptOfferAsync(int offerId, int customerId);
    }
}