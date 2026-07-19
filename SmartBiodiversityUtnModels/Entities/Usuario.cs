using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public string IdUsuario { get; set; }

        [Required]
        [ForeignKey(nameof(Rol))]
        public string IdRolesU { get; set; }

        [Required]
        [MaxLength(50)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string Estado { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }

        public int IntentosFallidos { get; set; }

        public DateTime? BloqueoHasta { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        // Propiedades de navegación
        public Rol Rol { get; set; } = null!;
        public ICollection<Bitacora> Bitacoras { get; set; } = new List<Bitacora>();
        public ICollection<Aporte> Aportes { get; set; } = new List<Aporte>();
        public ICollection<HistorialContrasena> HistorialContrasenas { get; set; } = new List<HistorialContrasena>();
        public ICollection<Token> Tokens { get; set; } = new List<Token>();
    }
}
