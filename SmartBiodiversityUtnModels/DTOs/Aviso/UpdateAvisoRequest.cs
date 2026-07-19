using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aviso
{
    public class UpdateAvisoRequest
    {
        public string? TituloAvi { get; set; }
        public string? MensajeAvi { get; set; }
        public string? CategoriaAvi { get; set; }
        public bool? ActivoAvi { get; set; }
        public DateTime? FechaIniAvi { get; set; }
        public DateTime? FechaFinAvi { get; set; }
    }
}
