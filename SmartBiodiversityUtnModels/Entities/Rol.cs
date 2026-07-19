using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Roles")]
    public class Rol
    {
        [Key]
        public string IdRoles { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [MaxLength(50)]
        public string NombreRol { get; set; } = string.Empty;

        // Propiedades de navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<Bitacora> Bitacoras { get; set; } = new List<Bitacora>();
        public ICollection<Aviso> Avisos { get; set; } = new List<Aviso>();
    }
}
