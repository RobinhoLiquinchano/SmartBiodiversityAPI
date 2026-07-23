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

        // ====== Cambiar facultad de la especie (opcional) ======
        // Si se envía un IdFacultad, se actualiza la vinculación.
        // Si se envía vacío ("") o null, se elimina la vinculación actual.
        public string? FacultadId { get; set; }

        // ====== NUEVO: Detalle ampliado opcional ======
        // Si viene un objeto, se crea o actualiza (upsert) el detalle correspondiente.
        // Si viene null, el detalle NO se toca (se conserva lo que hubiera).
        public DetalleFloraDto? DetalleFlora { get; set; }
        public DetalleFaunaDto? DetalleFauna { get; set; }
    }
}
