using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Multimedia;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultimediasController(IMultimediaService multimediaService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MultimediaResponse>>> GetAll()
            => Ok(await multimediaService.GetMultimediaByEspecieIdAsync());

        [HttpGet("{especieId}")]
        [AllowAnonymous]
        public async Task<ActionResult<MultimediaResponse>> GetByEspecie(string especieId)
        {
            var result = await multimediaService.GetMultimediaByEspecieIdAsync(especieId);
            return result is null ? NotFound() : Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<MultimediaResponse>> Upload([FromForm] CreateMultimediaRequest request)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            try
            {
                var result = await multimediaService.AddMultimediaAsync(request, idUsuario);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(string id)
        {
            var idUsuario = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(idUsuario)) return Unauthorized();

            var deleted = await multimediaService.DeleteMultimediaAsync(id, idUsuario);
            return deleted ? NoContent() : NotFound();
        }
    }
}