using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Aportes")]
    public class Aporte
    {
        [Key]
        public string IdAportes { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string IdUsuarioApo { get; set; }

        [Required]
        [MaxLength(250)]
        public string DescripcionApo { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? RutaImagenApo { get; set; }

        [Required]
        [MaxLength(20)]
        public string EstadoApo { get; set; } = string.Empty;

        public DateTime FechaApo { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
