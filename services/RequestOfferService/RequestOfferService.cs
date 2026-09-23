using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.data;
using RequestLifeCycle.DTOs.RequestOffer;
using RequestLifeCycle.Entities;
using RequestLifeCycle.Enums;
using RequestLifeCycle.services;
using services.CashingServices;

namespace RequestLifeCycle.services
{
    public class RequestOfferService(AppDbContext Context, ICaching CachingService) : IRequestOfferService
    {
        

        public async Task<OfferResponseDto> CreateOfferAsync(int userId, CreateOfferDto dto)
        {
            // 1. Fetch RepairShop associated with current authenticated user
            var repairShop = await Context.RepairShops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (repairShop == null)
                throw new KeyNotFoundException("بيانات محل الصيانة غير موجودة لهذا المستخدم.");

            // 2. Verify ServiceRequest existence and status
            var request = await Context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.id == dto.ServiceRequestId);

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("لا يمكن تقديم عرض على طلب ليس في حالة Pending.");

            // 3. Ensure repair shop hasn't submitted an offer for this request before
            bool alreadyOffered = await Context.RequestOffers
                .AnyAsync(o => o.ServiceRequestId == dto.ServiceRequestId && o.RepairShopId == repairShop.Id);

            if (alreadyOffered)
                throw new InvalidOperationException("لقد قمت بتقديم عرض على هذا الطلب من قبل.");

            // 4. Create Offer
            var offer = new RequestOffer
            {
                ServiceRequestId = dto.ServiceRequestId,
                RepairShopId = repairShop.Id,
                OfferedPrice = dto.OfferedPrice,
                Description = dto.Description,
                Status = OfferStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            Context.RequestOffers.Add(offer);
            await Context.SaveChangesAsync();
            await CachingService.RemoveAsync($"offers_request_{dto.ServiceRequestId}");
            return new OfferResponseDto
            {
                Id = offer.Id,
                ServiceRequestId = offer.ServiceRequestId,
                RepairShopId = offer.RepairShopId,
                ShopName = repairShop.ShopName,
                OfferedPrice = offer.OfferedPrice,
                Description = offer.Description,
                Status = offer.Status,
                CreatedAt = offer.CreatedAt
            };
        }

        public async Task<List<OfferResponseDto>> GetOffersForRequestAsync(int requestId, int customerId)
        {
            // 1. Verify Ownership of Request
            var request = await Context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.id == requestId);

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (request.CustomerId != customerId)
                throw new UnauthorizedAccessException("غير مصرح لك برؤية العروض الخاصة بهذا الطلب.");
            var cacheKey = $"offers_request_{requestId}";
            var offers = await CachingService.GetOrCreateAsync(cacheKey, fetchFunction: () => FetchOffersFromDbAsync(requestId));
            // 2. Projection & Read
            return offers;
        }

        private async Task<List<OfferResponseDto>> FetchOffersFromDbAsync(int requestId)
        {
            return await Context.RequestOffers
                .AsNoTracking()
                .Where(o => o.ServiceRequestId == requestId)
                .OrderBy(o => o.OfferedPrice)
                .Select(o => new OfferResponseDto
                {
                    Id = o.Id,
                    ServiceRequestId = o.ServiceRequestId,
                    RepairShopId = o.RepairShopId,
                    ShopName = o.RepairShop.ShopName,
                    OfferedPrice = o.OfferedPrice,
                    Description = o.Description,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();
        }

        public async Task AcceptOfferAsync(int offerId, int customerId)
        {
            var selectedOffer = await Context.RequestOffers
                .Include(o => o.ServiceRequest)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (selectedOffer == null)
                throw new KeyNotFoundException("العرض غير موجود.");

            var request = selectedOffer.ServiceRequest;

            if (request.CustomerId != customerId)
                throw new UnauthorizedAccessException("لا يمكنك قبول عرض لطلب لا تملكه.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("الطلب ليس في حالة Pending لقبول عروض جديدة.");

            if (selectedOffer.Status != OfferStatus.Pending)
                throw new InvalidOperationException("هذا العرض لم يعد متاحًا للقبول.");

            using var transaction = await Context.Database.BeginTransactionAsync();
            try
            {
                selectedOffer.Status = OfferStatus.Accepted;

                await Context.RequestOffers
                    .Where(o => o.ServiceRequestId == request.id && o.Id != offerId && o.Status == OfferStatus.Pending)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OfferStatus.Rejected));
                request.Status = RequestStatus.Accepted;

                await Context.SaveChangesAsync();
                await transaction.CommitAsync();
                await CachingService.RemoveAsync($"offers_request_{request.id}");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

          
    }
}