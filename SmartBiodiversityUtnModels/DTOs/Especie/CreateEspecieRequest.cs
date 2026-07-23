using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using SmartBiodiversityUtnModels.DTOs.Especie;

namespace SmartBiodiversityUtnModels.DTOs
{
    public class CreateEspecieRequest
    {
        public string NombreComun { get; set; } = string.Empty;
        public string NombreCientifico { get; set; } = string.Empty;
        [StringLength(2000, ErrorMessage = "La descripción no puede exceder los 2000 caracteres.")]
        public string? Descripcion { get; set; }
        public string? Habitat { get; set; }
        public string CategoriaId { get; set; } = string.Empty;

        // ====== Facultad a la que se vincula la especie (opcional) ======
        public string? FacultadId { get; set; }

        // ====== NUEVO: Detalle ampliado opcional ======
        // Envía solo el que corresponda al tipo de especie (o ninguno).
        // Si ambos vienen null, la especie se crea sin detalle (comportamiento actual).
        public DetalleFloraDto? DetalleFlora { get; set; }
        public DetalleFaunaDto? DetalleFauna { get; set; }
    }
}
