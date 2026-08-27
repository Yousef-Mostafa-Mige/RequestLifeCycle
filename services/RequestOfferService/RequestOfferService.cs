using Microsoft.EntityFrameworkCore;
using RequestLifeCycle.data;
using RequestLifeCycle.DTOs.RequestOffer;
using RequestLifeCycle.Entities;
using RequestLifeCycle.Enums;
using RequestLifeCycle.services;

namespace RequestLifeCycle.services
{
    public class RequestOfferService : IRequestOfferService
    {
        private readonly AppDbContext _context;

        public RequestOfferService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OfferResponseDto> CreateOfferAsync(int userId, CreateOfferDto dto)
        {
            // 1. Fetch RepairShop associated with current authenticated user
            var repairShop = await _context.RepairShops
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (repairShop == null)
                throw new KeyNotFoundException("بيانات محل الصيانة غير موجودة لهذا المستخدم.");

            // 2. Verify ServiceRequest existence and status
            var request = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dto.ServiceRequestId);

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("لا يمكن تقديم عرض على طلب ليس في حالة Pending.");

            // 3. Ensure repair shop hasn't submitted an offer for this request before
            bool alreadyOffered = await _context.RequestOffers
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

            _context.RequestOffers.Add(offer);
            await _context.SaveChangesAsync();

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

        public async Task<IEnumerable<OfferResponseDto>> GetOffersForRequestAsync(int requestId, int customerId)
        {
            // 1. Verify Ownership of Request
            var request = await _context.ServiceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null)
                throw new KeyNotFoundException("الطلب غير موجود.");

            if (request.CustomerId != customerId)
                throw new UnauthorizedAccessException("غير مصرح لك برؤية العروض الخاصة بهذا الطلب.");

            // 2. Projection & Read
            return await _context.RequestOffers
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
            // 1. Load offer with parent ServiceRequest
            var selectedOffer = await _context.RequestOffers
                .Include(o => o.ServiceRequest)
                .FirstOrDefaultAsync(o => o.Id == offerId);

            if (selectedOffer == null)
                throw new KeyNotFoundException("العرض غير موجود.");

            var request = selectedOffer.ServiceRequest;

            // 2. Verification Rules
            if (request.CustomerId != customerId)
                throw new UnauthorizedAccessException("لا يمكنك قبول عرض لطلب لا تملكه.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("الطلب ليس في حالة Pending لقبول عروض جديدة.");

            if (selectedOffer.Status != OfferStatus.Pending)
                throw new InvalidOperationException("هذا العرض لم يعد متاحًا للقبول.");

            // 3. Database Transaction execution
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Accept chosen offer
                selectedOffer.Status = OfferStatus.Accepted;

                // Bulk update all remaining pending offers to Rejected in database directly
                await _context.RequestOffers
                    .Where(o => o.ServiceRequestId == request.Id && o.Id != offerId && o.Status == OfferStatus.Pending)
                    .ExecuteUpdateAsync(s => s.SetProperty(o => o.Status, OfferStatus.Rejected));

                // Update request status
                request.Status = RequestStatus.Accepted;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}