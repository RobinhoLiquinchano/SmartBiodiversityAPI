using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Especie
{
    public class EspecieResponse
    {
        public string IdEspecie { get; set; } = string.Empty;
        public string NombreComun { get; set; } = string.Empty;
        public string NombreCientifico { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Habitat { get; set; }
        public EstadoEspecie EstadoEsp { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public DateTime FechaRegistroEsp { get; set; }
    }
}
