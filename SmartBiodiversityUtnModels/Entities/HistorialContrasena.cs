using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("HistorialContra")]
    public class HistorialContrasena
    {
        [Key]
        public string IdHistorialHco { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [ForeignKey(nameof(Usuario))]
        public string IdUsuarioHco { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string PasswordHashHco { get; set; } = string.Empty;

        public DateTime FechaHco { get; set; }

        // Propiedad de navegación
        public Usuario Usuario { get; set; } = null!;
    }
}
