using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres.")]
        public string Nombres { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido es obligatorio.")]
        [MaxLength(50, ErrorMessage = "El apellido no puede exceder 50 caracteres.")]
        public string Apellidos { get; set; } = string.Empty;
    }
}
