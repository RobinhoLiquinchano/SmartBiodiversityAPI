using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBiodiversityUtnModels.Entities
{
    /// <summary>
    /// Detalle zoológico opcional (1:1) para especies de FAUNA.
    /// Todas las columnas son nullables: una especie puede existir sin este detalle,
    /// por lo que agregar esta tabla NO rompe las especies ya registradas.
    /// </summary>
    [Table("DetallesFauna")]
    public class DetalleFauna
    {
        // La PK es a la vez la FK hacia Especie (relación 1:1 compartiendo la clave)
        [Key]
        [ForeignKey(nameof(Especie))]
        public string IdEspecies { get; set; } = string.Empty;

        [Column(TypeName = "decimal(6,2)")]
        public decimal? LongitudPromedioCm { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? EnvergaduraCm { get; set; }

        [Column(TypeName = "decimal(8,2)")]
        public decimal? PesoPromedioGramos { get; set; }

        [MaxLength(200)]
        public string? TipoPelajePlumaje { get; set; }

        [MaxLength(300)]
        public string? DimorfismoSexual { get; set; }

        [MaxLength(100)]
        public string? Dieta { get; set; }

        [MaxLength(50)]
        public string? PatronActividad { get; set; }

        // Propiedad de navegación
        public Especie Especie { get; set; } = null!;
    }
}
