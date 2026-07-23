using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Facultades")]
    public class Facultad
    {
        [Key]
        public string IdFacultad { get; set; } = "FAC-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();

        // ====== NUEVO: Número visible en el mapa y app móvil ======
        // Fijo, secuencial (1, 2, 3...). Sirve como referencia estable
        // para que la app móvil identifique la facultad por número.
        [Required]
        public int NumeroFac { get; set; }

        [Required]
        [MaxLength(150)]
        public string NombreFac { get; set; } = string.Empty;

        [Required]
        public double Latitud { get; set; }

        [Required]
        public double Longitud { get; set; }

        [MaxLength(300)]
        public string? DescripcionFac { get; set; }

        // Propiedad de navegación N:M
        public ICollection<EspecieFacultad> EspecieFacultades { get; set; } = new List<EspecieFacultad>();
    }
}
