using System.ComponentModel.DataAnnotations;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class UserDto
    {
        [Required]
        [MaxLength(50)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El código de verificación es obligatorio.")]
        [StringLength(6, MinimumLength = 6,
            ErrorMessage = "El código debe tener 6 dígitos.")]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "El código debe contener únicamente 6 números.")]
        public string CodigoVerificacion { get; set; } = string.Empty;
    }
}