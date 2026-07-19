using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aviso
{
    public class AvisoResponse
    {
        public string IdAvisos { get; set; } = string.Empty;
        public string IdRolesAvi { get; set; } = string.Empty;
        public string TituloAvi { get; set; } = string.Empty;
        public string MensajeAvi { get; set; } = string.Empty;
        public string? CategoriaAvi { get; set; }
        public bool ActivoAvi { get; set; }
        public DateTime FechaIniAvi { get; set; }
        public DateTime? FechaFinAvi { get; set; }
        public string? NombreRol { get; set; }
    }
}
