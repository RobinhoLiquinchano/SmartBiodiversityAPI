using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Categoria;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController(ICategoriaService categoriaService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoriaResponse>>> GetCategorias()
        => Ok(await categoriaService.GetAllCategoriasAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<CategoriaResponse>> GetCategoriaById(string id)
        {
            var categoria = await categoriaService.GetCategoriaByIdAsync(id);
            return categoria is null ? NotFound("No se encontró la categoria con el ID proporcionado.") : Ok(categoria);
        }

        [HttpPost]
        public async Task<ActionResult<CategoriaResponse>> AddCategoria(CreateCategoriaRequest categoria)
        {
            var createdCategoria = await categoriaService.CreateCategoriaAsync(categoria);
            return CreatedAtAction(nameof(GetCategoriaById), new { id = createdCategoria.Id }, createdCategoria);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateCategoria(string id, UpdateCategoriaRequest categoria)
        {
            var updated = await categoriaService.UpdateCategoriaAsync(id, categoria);
            return updated ? NoContent() : NotFound("No se encontró la categoria con el ID proporcionado.");
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteCategoria(string id)
        {
            var deleted = await categoriaService.DeleteCategoriaAsync(id);
            return deleted ? NoContent() : NotFound("No se encontró la categoria con el ID proporcionado.");
        }
    }
}
