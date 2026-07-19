using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Bitacora;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacorasController(IBitacoraService bitacoraService) : ControllerBase
    {
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetAll()
        {
            var bitacoras = await bitacoraService.GetAllBitacorasAsync();
            return Ok(bitacoras);
        }

        [HttpGet("mis-acciones")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetMisAcciones()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var bitacoras = await bitacoraService.GetBitacorasByUsuarioAsync(userId);
            return Ok(bitacoras);
        }

        [HttpGet("usuario/{idUsuario}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetByUsuario(string idUsuario)
        {
            var bitacoras = await bitacoraService.GetBitacorasByUsuarioAsync(idUsuario);
            return Ok(bitacoras);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")] 
        public async Task<ActionResult<BitacoraResponse>> Create(CreateBitacoraRequest request)
        {
            var created = await bitacoraService.CreateBitacoraAsync(request);
            return Ok(created);
        }
    }
}