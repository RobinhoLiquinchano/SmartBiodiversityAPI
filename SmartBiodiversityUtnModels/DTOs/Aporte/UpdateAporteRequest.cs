using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aporte
{
    public class UpdateAporteRequest
    {
        public string TituloApo { get; set; } = string.Empty;
        public string? DescripcionApo { get; set; }
        public string? RutaArchivoApo { get; set; }
    }
}
