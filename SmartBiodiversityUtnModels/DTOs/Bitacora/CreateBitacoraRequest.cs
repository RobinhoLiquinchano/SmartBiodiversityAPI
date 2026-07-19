using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Bitacora
{
    public class CreateBitacoraRequest
    {
        public string IdUsuarioBit { get; set; } = string.Empty;
        public string AccionBit { get; set; } = string.Empty;
        public string? DetalleBit { get; set; }
    }
}
