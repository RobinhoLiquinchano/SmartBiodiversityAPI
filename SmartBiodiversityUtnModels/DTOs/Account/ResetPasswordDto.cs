using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    using System.ComponentModel.DataAnnotations;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "El código es obligatorio.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
            ErrorMessage = "Debe incluir mayúsculas, minúsculas, números y un carácter especial.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
}
