using System.ComponentModel.DataAnnotations;

public class UserDto
{
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

    [Required(ErrorMessage = "El código de verificación es obligatorio.")]
    [StringLength(6, MinimumLength = 6,
        ErrorMessage = "El código debe tener 6 dígitos.")]
    public string CodigoVerificacion { get; set; } = string.Empty;
}