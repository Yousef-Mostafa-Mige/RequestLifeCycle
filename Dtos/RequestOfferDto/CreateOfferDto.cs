using System.ComponentModel.DataAnnotations;

namespace RequestLifeCycle.DTOs.RequestOffer
{
    public class CreateOfferDto
    {
        [Required(ErrorMessage = "معرف الطلب مطلوب")]
        public int ServiceRequestId { get; set; }

        [Required(ErrorMessage = "السعر مطلوب")]
        [Range(1, 100000, ErrorMessage = "السعر يجب أن يكون أكبر من 0")]
        public decimal OfferedPrice { get; set; }

        [StringLength(500, ErrorMessage = "الوصف لا يمكن أن يتجاوز 500 حرف")]
        public string? Description { get; set; }
    }
}