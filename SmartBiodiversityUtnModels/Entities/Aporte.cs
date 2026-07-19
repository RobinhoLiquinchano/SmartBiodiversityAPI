using SmartBiodiversityUtnModels.DTOs;
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
        public string IdAporte { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string IdUsuarioApo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string TituloApo { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DescripcionApo { get; set; }

        //[Required]
        //[MaxLength(100)]
        //public string TipoApo { get; set; } = string.Empty; // Foto, Información, Video, etc.

        [MaxLength(300)]
        public string? RutaArchivoApo { get; set; }

        [Required]
        public EstadoAporte EstadoApo { get; set; } = EstadoAporte.Pendiente;

        public DateTime FechaCreacionApo { get; set; }

        public DateTime? FechaAprobacionApo { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}


