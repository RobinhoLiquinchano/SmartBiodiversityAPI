using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspeciesController(IEspecieService especieService) : ControllerBase
    {
        // Solo Administradores pueden crear especies
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<EspecieResponse>> AddEspecie(CreateEspecieRequest especie)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var createdEspecie = await especieService.AddEspecieAsync(especie, idUsuario);
            return CreatedAtAction(nameof(GetEspecieById), new { id = createdEspecie.IdEspecie }, createdEspecie);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<EspecieResponse>>> GetAllEspecies()
            => Ok(await especieService.GetAllEspeciesAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<EspecieResponse>> GetEspecieById(string id)
        {
            var especieResponse = await especieService.GetEspecieByIdAsync(id);
            return especieResponse is null ? NotFound("No se encontró la especie con el ID proporcionado.") : Ok(especieResponse);
        }

        // Solo Administradores pueden editar
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> UpdateEspecie(string id, UpdateEspecieRequest especie)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var updated = await especieService.UpdateEspecieAsync(id, especie, idUsuario);
            return updated ? NoContent() : NotFound("No se encontró la especie con el ID proporcionado.");
        }

        // Solo Administradores pueden eliminar
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> DeleteEspecie(string id)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario))
                return Unauthorized("No se pudo obtener el ID del usuario.");

            var deleted = await especieService.DeleteEspecieAsync(id, idUsuario);
            return deleted ? NoContent() : NotFound("No se encontró la especie con el ID proporcionado.");
        }
    }
}