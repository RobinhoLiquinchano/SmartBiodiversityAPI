using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Categorias")]
    public class Categoria
    {
        [Key]
        public string IdCategorias { get; set; } = Guid.NewGuid().ToString("N");

        [Required]
        [MaxLength(100)]
        public string NombreCat { get; set; } = string.Empty;

        // Propiedad de navegación
        public ICollection<Especie> Especies { get; set; } = new List<Especie>();
    }
}
