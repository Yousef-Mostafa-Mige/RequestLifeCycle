using System.ComponentModel.DataAnnotations;

namespace RequestLifeCycle.DTOs.ServiceRequest
{
    public class CreateServiceRequestDto
    {
        [Required(ErrorMessage = "وصف المشكلة مطلوب")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "الوصف يجب أن يكون بين 10 و 1000 حرف")]
        public string Description { get; set; } = string.Empty;

        [Range(0, 100000, ErrorMessage = "السعر المقترح يجب أن يكون 0 أو أكثر")]
        public decimal ProposedPrice { get; set; }
    }
}