using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Bitacora;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController(IBitacoraService bitacoraService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetAll()
        {
            var bitacoras = await bitacoraService.GetAllBitacorasAsync();
            return Ok(bitacoras);
        }

        [HttpGet("usuario/{idUsuario}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetByUsuario(string idUsuario)
        {
            var bitacoras = await bitacoraService.GetBitacorasByUsuarioAsync(idUsuario);
            return Ok(bitacoras);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BitacoraResponse>> Create(CreateBitacoraRequest request)
        {
            try
            {
                var created = await bitacoraService.CreateBitacoraAsync(request);
                return CreatedAtAction(nameof(GetByUsuario), new { idUsuario = created.IdUsuarioBit }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}