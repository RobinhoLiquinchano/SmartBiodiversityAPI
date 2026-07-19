using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Multimedia")]
    public class Multimedia
    {
        [Key]
        public string IdMultimedia { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Especie))]
        public string IdEspeciesMul { get; set; }

        [Required]
        [MaxLength(50)]
        public string TipoArchivoMul { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string RutaArchivoMul { get; set; } = string.Empty;

        public DateTime FechaMul { get; set; }

        // Propiedad de navegación
        public Especie Especie { get; set; } = null!;
    }
}
