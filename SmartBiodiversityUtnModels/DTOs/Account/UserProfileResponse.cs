using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class UserProfileResponse
    {
        public string IdUsuario { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }
}
