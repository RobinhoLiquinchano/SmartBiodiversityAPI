using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Aviso;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvisosController(IAvisoService avisoService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvisoResponse>>> GetAll()
        {
            var avisos = await avisoService.GetAllAvisosAsync();
            return Ok(avisos);
        }

        [HttpGet("activos")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<AvisoResponse>>> GetActivos()
        {
            var avisos = await avisoService.GetAvisosActivosAsync();
            return Ok(avisos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<AvisoResponse>> GetById(string id)
        {
            var aviso = await avisoService.GetAvisoByIdAsync(id);
            if (aviso == null)
                return NotFound(new { message = "Aviso no encontrado." });

            return Ok(aviso);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<AvisoResponse>> Create(CreateAvisoRequest request)
        {
            var createdAviso = await avisoService.CreateAvisoAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = createdAviso.IdAvisos }, createdAviso);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(string id, UpdateAvisoRequest request)
        {
            var success = await avisoService.UpdateAvisoAsync(id, request);
            if (!success)
                return NotFound(new { message = "Aviso no encontrado o no se pudo actualizar." });

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await avisoService.DeleteAvisoAsync(id);
            if (!success)
                return NotFound(new { message = "Aviso no encontrado." });

            return NoContent();
        }
    }
}
