using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("EspecieFacultad")]
    public class EspecieFacultad
    {
        [Key]
        public string IdEspecieFacultad { get; set; } = "EF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

        [Required]
        [ForeignKey(nameof(Especie))]
        public string IdEspecies { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Facultad))]
        public string IdFacultad { get; set; } = string.Empty;

        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        public Especie Especie { get; set; } = null!;
        public Facultad Facultad { get; set; } = null!;
    }
}
