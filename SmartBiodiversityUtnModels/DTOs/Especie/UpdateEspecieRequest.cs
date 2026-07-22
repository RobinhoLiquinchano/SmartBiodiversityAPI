using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Especie
{
    public class UpdateEspecieRequest
    {
        public string? NombreComun { get; set; }
        public string? NombreCientifico { get; set; }
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres.")]
        public string? Descripcion { get; set; }
        public string? Habitat { get; set; }
        public EstadoEspecie? EstadoEsp { get; set; }
        public string? IdCategoria { get; set; }
    }
}
