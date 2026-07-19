using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Aviso;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvisosController(IAvisoService avisoService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvisoResponse>>> GetAll()
            => Ok(await avisoService.GetAllAvisosAsync());

        [HttpGet("activos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvisoResponse>>> GetActivos()
            => Ok(await avisoService.GetAvisosActivosAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AvisoResponse>> GetById(string id)
        {
            var aviso = await avisoService.GetAvisoByIdAsync(id);
            return aviso is null ? NotFound("Aviso no encontrado.") : Ok(aviso);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<AvisoResponse>> Create(CreateAvisoRequest request)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var created = await avisoService.CreateAvisoAsync(request, idUsuario);
            return CreatedAtAction(nameof(GetById), new { id = created.IdAvisos }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(string id, UpdateAvisoRequest request)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var success = await avisoService.UpdateAvisoAsync(id, request, idUsuario);
            return success ? NoContent() : NotFound("Aviso no encontrado.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(string id)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var success = await avisoService.DeleteAvisoAsync(id, idUsuario);
            return success ? NoContent() : NotFound("Aviso no encontrado.");
        }
    }
}