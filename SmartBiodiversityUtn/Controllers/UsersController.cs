using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Account;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers;


[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet("mi-perfil")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile()
    {
        var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idUsuario))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        var perfil = await _userService.GetProfileAsync(idUsuario);

        if (perfil == null)
            return NotFound(new { message = "Usuario no encontrado." });

        return Ok(perfil);
    }

    [HttpPut("mi-perfil")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMyProfile(UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(idUsuario))
            return Unauthorized(new { message = "No se pudo identificar al usuario." });

        var success = await _userService.UpdateProfileAsync(idUsuario, request);

        if (!success)
            return BadRequest(new { message = "No se pudo actualizar el perfil. Verifique los datos." });

        // Obtener el perfil actualizado para retornarlo
        var perfilActualizado = await _userService.GetProfileAsync(idUsuario);
        return Ok(new
        {
            message = "Perfil actualizado exitosamente.",
            perfil = perfilActualizado
        });
    }

    [HttpGet("listar-todos")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(IEnumerable<UserListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        var usuarios = await _userService.GetAllUsersAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(typeof(UserListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(string id)
    {
        var usuario = await _userService.GetUserByIdAsync(id);

        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado." });

        return Ok(usuario);
    }

    [HttpGet("estadisticas")]
    [Authorize(Roles = "Administrador")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserStatistics()
    {
        var usuarios = await _userService.GetAllUsersAsync();
        var listaUsuarios = usuarios.ToList();

        return Ok(new
        {
            total = listaUsuarios.Count,
            activos = listaUsuarios.Count(u => u.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase)),
            inactivos = listaUsuarios.Count(u => u.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase)),
            visitantes = listaUsuarios.Count(u => u.NombreRol == "Visitante"),
            administradores = listaUsuarios.Count(u => u.NombreRol == "Administrador")
        });
    }
}
