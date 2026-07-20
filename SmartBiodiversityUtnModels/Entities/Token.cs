using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartBiodiversityUtnModels.Entities
{
    [Table("Tokens")]
    public class Token
    {
        [Key]
        public string IdTokens { get; set; } =
            "TOK-" + Guid.NewGuid().ToString("N")
                .Substring(0, 10).ToUpper();

        /*
         * Para tokens de registro, el usuario todavía no existe,
         * por eso esta propiedad debe poder ser null.
         */
        [ForeignKey(nameof(Usuario))]
        public string? IdUsuarioTok { get; set; }

        /*
         * Correo usado para localizar el token antes de que
         * exista un usuario registrado.
         */
        [MaxLength(100)]
        [EmailAddress]
        public string? CorreoTok { get; set; }

        /*
         * Aquí se guardará el HASH del código de 6 dígitos,
         * no el código real.
         */
        [Required]
        [MaxLength(100)]
        public string CodigoTok { get; set; } = string.Empty;

        /*
         * Valores sugeridos:
         * - Registro
         * - Reset
         */
        [Required]
        [MaxLength(10)]
        public string TipoTok { get; set; } = string.Empty;

        public DateTime FechaCreacionTok { get; set; }

        public DateTime FechaExpiracionTok { get; set; }

        /*
         * Valores:
         * "0" = disponible
         * "1" = utilizado
         */
        [MaxLength(10)]
        public string? Usado { get; set; } = "0";

        // Es null para tokens creados antes del registro.
        public Usuario? Usuario { get; set; }
    }
}