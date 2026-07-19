using SmartBiodiversityUtnModels.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Especies")]
    public class Especie
    {
        [Key]
        public string IdEspecies { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Categoria))]
        public string IdCategoriaEsp { get; set; }

        [Required]
        [MaxLength(50)]
        public string NombreComunEsp { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NombreCientificoEsp { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? DescripcionEsp { get; set; }

        [MaxLength(200)]
        public string? HabitatEsp { get; set; }
        public EstadoEspecie EstadoEsp { get; set; } = EstadoEspecie.Activo;

        public DateTime FechaRegistroEsp { get; set; }

        // Propiedades de navegación
        public Categoria Categoria { get; set; } = null!;
        public ICollection<Multimedia> MultimediaArchivos { get; set; } = new List<Multimedia>();
    }
}
