using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Aporte
{
    public class CreateAporteRequest
    {
        public string TituloApo { get; set; } = string.Empty;
        public string? DescripcionApo { get; set; }

        // NOTA: RutaArchivoApo NO va aquí. La URL la genera el servidor
        // al subir el archivo a Supabase (carpeta Aportes).
    }
}
