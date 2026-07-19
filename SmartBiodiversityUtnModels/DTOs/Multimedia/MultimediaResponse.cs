using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Multimedia
{
    public class MultimediaResponse
    {
        public string IdMultimedia { get; set; } = string.Empty;
        public string EspecieId { get; set; } = string.Empty;
        public string TipoArchivo { get; set; } = string.Empty;
        public string RutaArchivo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }
}
