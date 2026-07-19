using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Bitacora")]
    public class Bitacora
    {
        [Key]
        public string IdLog { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string IdUsuarioBit { get; set; }

        [Required]
        [ForeignKey(nameof(Rol))]
        public string IdRolesBit { get; set; }

        [Required]
        [MaxLength(50)]
        public string AccionBit { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? DetalleBit { get; set; }

        public DateTime FechaBit { get; set; }

        // Propiedades de navegación
        public Usuario Usuario { get; set; } = null!;
        public Rol Rol { get; set; } = null!;
    }
}
