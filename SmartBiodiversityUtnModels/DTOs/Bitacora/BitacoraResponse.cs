using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Bitacora
{
    public class BitacoraResponse
    {
        public string IdLog { get; set; } = string.Empty;
        public string IdUsuarioBit { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string IdRolesBit { get; set; } = string.Empty;
        public string NombreRol { get; set; } = string.Empty;
        public string AccionBit { get; set; } = string.Empty;
        public string? DetalleBit { get; set; }
        public DateTime FechaBit { get; set; }
    }
}
