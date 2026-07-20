using System.ComponentModel.DataAnnotations;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class SendVerificationCodeRequest
    {
        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
        public string Email { get; set; } = string.Empty;
    }
}