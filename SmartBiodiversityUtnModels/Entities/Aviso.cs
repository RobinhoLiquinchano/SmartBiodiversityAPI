using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Avisos")]
    public class Aviso
    {
        [Key]
        public string IdAvisos { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Rol))]
        public string IdRolesAvi { get; set; }

        [Required]
        [MaxLength(100)]
        public string TituloAvi { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string MensajeAvi { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? CategoriaAvi { get; set; }

        public bool ActivoAvi { get; set; }

        public DateTime FechaIniAvi { get; set; }

        public DateTime? FechaFinAvi { get; set; }

        // Propiedad de navegación
        public Rol Rol { get; set; } = null!;
    }
}
