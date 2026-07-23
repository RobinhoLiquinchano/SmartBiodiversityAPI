using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBiodiversityUtnModels.Entities
{
    /// <summary>
    /// Detalle botánico opcional (1:1) para especies de FLORA.
    /// Todas las columnas son nullables: una especie puede existir sin este detalle,
    /// por lo que agregar esta tabla NO rompe las especies ya registradas.
    /// </summary>
    [Table("DetallesFlora")]
    public class DetalleFlora
    {
        // La PK es a la vez la FK hacia Especie (relación 1:1 compartiendo la clave)
        [Key]
        [ForeignKey(nameof(Especie))]
        public string IdEspecies { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AlturaPromedioM { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? AlturaMaximaM { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? DiametroTroncoCm { get; set; }

        [MaxLength(150)]
        public string? TipoCortezaTronco { get; set; }

        [MaxLength(100)]
        public string? FormaCopa { get; set; }

        [MaxLength(150)]
        public string? TipoHoja { get; set; }

        [MaxLength(150)]
        public string? ColorFlorFruto { get; set; }

        [MaxLength(100)]
        public string? HabitoCrecimiento { get; set; }

        // Propiedad de navegación
        public Especie Especie { get; set; } = null!;
    }
}
