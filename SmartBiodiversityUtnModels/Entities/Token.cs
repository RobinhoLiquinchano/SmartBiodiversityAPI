using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Tokens")]
    public class Token
    {
        [Key]
        public string IdTokens { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string IdUsuarioTok { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string CodigoTok { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string TipoTok { get; set; } = string.Empty;

        public DateTime FechaCreacionTok { get; set; }

        public DateTime FechaExpiracionTok { get; set; }

        [MaxLength(10)]
        public string? Usado { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
