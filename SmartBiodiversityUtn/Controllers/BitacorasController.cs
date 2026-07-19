using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Bitacora;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class BitacorasController(IBitacoraService bitacoraService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetAll()
        {
            var bitacoras = await bitacoraService.GetAllBitacorasAsync();
            return Ok(bitacoras);
        }

        [HttpGet("usuario/{idUsuario}")]
        public async Task<ActionResult<IEnumerable<BitacoraResponse>>> GetByUsuario(string idUsuario)
        {
            var bitacoras = await bitacoraService.GetBitacorasByUsuarioAsync(idUsuario);
            return Ok(bitacoras);
        }

        [HttpPost]
        public async Task<ActionResult<BitacoraResponse>> Create(CreateBitacoraRequest request)
        {
            var created = await bitacoraService.CreateBitacoraAsync(request);
            return Ok(created);
        }
    }
}
