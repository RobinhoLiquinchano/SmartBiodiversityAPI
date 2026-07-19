using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aporte
{
    public class AporteResponse
    {
        public string IdAporte { get; set; } = string.Empty;

        public string IdUsuarioApo { get; set; } = string.Empty;

        public string TituloApo { get; set; } = string.Empty;

        public string? DescripcionApo { get; set; }

        //public string TipoApo { get; set; } = string.Empty;

        public string? RutaArchivoApo { get; set; }

        public EstadoAporte EstadoApo { get; set; }

        public DateTime FechaCreacionApo { get; set; }

        public DateTime? FechaAprobacionApo { get; set; }

        public string? NombreUsuario { get; set; }

        public string? CorreoUsuario { get; set; }
    }
}
