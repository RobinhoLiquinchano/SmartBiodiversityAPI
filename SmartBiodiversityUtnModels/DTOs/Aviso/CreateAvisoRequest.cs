using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aviso
{
    public class CreateAvisoRequest
    {
        public string TituloAvi { get; set; } = string.Empty;
        public string MensajeAvi { get; set; } = string.Empty;
        public string? CategoriaAvi { get; set; }
        public DateTime? FechaFinAvi { get; set; }
    }
}
