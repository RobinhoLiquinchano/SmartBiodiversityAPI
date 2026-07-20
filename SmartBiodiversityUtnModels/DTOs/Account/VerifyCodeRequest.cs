using System.ComponentModel.DataAnnotations;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class VerifyCodeRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Codigo { get; set; } = string.Empty;
    }
}