using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs
{
    public class CreateEspecieRequest
    {
        public string NombreComun { get; set; } = string.Empty;
        public string NombreCientifico { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Habitat { get; set; }
        public string CategoriaId { get; set; } = string.Empty;
    }
}
