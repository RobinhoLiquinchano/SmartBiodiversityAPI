using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Categoria;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<CategoriaResponse>>> GetCategorias()
            => Ok(await categoriaService.GetAllCategoriasAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<CategoriaResponse>> GetCategoriaById(string id)
        {
            var categoria = await categoriaService.GetCategoriaByIdAsync(id);
            return categoria is null
                ? NotFound("No se encontró la categoría con el ID proporcionado.")
                : Ok(categoria);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<CategoriaResponse>> AddCategoria(CreateCategoriaRequest categoria)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var created = await categoriaService.CreateCategoriaAsync(categoria, idUsuario);
            return CreatedAtAction(nameof(GetCategoriaById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> UpdateCategoria(string id, UpdateCategoriaRequest categoria)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var updated = await categoriaService.UpdateCategoriaAsync(id, categoria, idUsuario);
            return updated ? NoContent() : NotFound("No se encontró la categoría.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> DeleteCategoria(string id)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var deleted = await categoriaService.DeleteCategoriaAsync(id, idUsuario);
            return deleted ? NoContent() : NotFound("No se encontró la categoría.");
        }
    }
}