using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AportesController(IAporteService aporteService) : ControllerBase
    {
        // Cualquier usuario autenticado puede crear
        [HttpPost("crear")]
        [Authorize]
        public async Task<IActionResult> CreateAporte(CreateAporteRequest request)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario.");

            var createdAporte = await aporteService.CreateAporteAsync(idUsuario, request);
            if (createdAporte == null) return BadRequest("No se pudo crear el aporte.");

            return CreatedAtAction(nameof(GetAporteById), new { id = createdAporte.IdAporte }, createdAporte);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAporteById(string id)
        {
            var aporte = await aporteService.GetAporteByIdAsync(id);
            return aporte != null ? Ok(aporte) : NotFound("Aporte no encontrado.");
        }

        // Solo Administrador puede ver todos
        [HttpGet("listar/todos")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> GetAllAportes()
        {
            return Ok(await aporteService.GetAllAportesAsync());
        }

        // Usuario autenticado ve sus propios aportes
        [HttpGet("mis-aportes")]
        [Authorize]
        public async Task<ActionResult> GetMyAportes()
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (idUsuario == null) return Unauthorized("No se pudo identificar al usuario.");

            return Ok(await aporteService.GetAportesByUsuarioAsync(idUsuario));
        }

        [HttpGet("usuario/{idUsuario}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAportesByUsuario(string idUsuario)
        {
            return Ok(await aporteService.GetAportesByUsuarioAsync(idUsuario));
        }

        [HttpGet("por-estado/{estado}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetAportesByEstado(EstadoAporte estado)
        {
            return Ok(await aporteService.GetAportesByEstadoAsync(estado));
        }

        // Solo Administrador puede actualizar
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> UpdateAporte(string id, UpdateAporteRequest request)
        {
            var success = await aporteService.UpdateAporteAsync(id, request);
            return success ? Ok("Aporte actualizado exitosamente.") : BadRequest("No se pudo actualizar el aporte.");
        }

        // Solo Administrador puede eliminar
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> DeleteAporte(string id)
        {
            var success = await aporteService.DeleteAporteAsync(id);
            return success ? NoContent() : BadRequest("No se pudo eliminar el aporte.");
        }

        // Solo Administrador
        [HttpPut("{id}/aprobar")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> ApproveAporte(string id)
        {
            var success = await aporteService.ApprovedAporteAsync(id);
            return success ? Ok("Aporte aprobado.") : BadRequest("Error al aprobar.");
        }

        // Solo Administrador
        [HttpPut("{id}/rechazar")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> RejectAporte(string id)
        {
            var success = await aporteService.RejectedAporteAsync(id);
            return success ? Ok("Aporte rechazado.") : BadRequest("Error al rechazar.");
        }

        [HttpGet("contar/{estado}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCountAportesByEstado(EstadoAporte estado)
        {
            var count = await aporteService.GetCountAportesByEstadoAsync(estado);
            return Ok(new { Total = count });
        }
    }
}