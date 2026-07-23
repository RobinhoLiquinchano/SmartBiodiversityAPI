using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Facultad;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacultadesController(IFacultadService facultadService) : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<FacultadResponse>>> GetAll()
            => Ok(await facultadService.GetAllFacultadesAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FacultadResponse>> GetById(string id)
        {
            var fac = await facultadService.GetFacultadByIdAsync(id);
            return fac is null ? NotFound("Facultad no encontrada.") : Ok(fac);
        }

        [HttpGet("{id}/especies")]
        [AllowAnonymous]
        public async Task<ActionResult<FacultadEspeciesResponse>> GetEspeciesPorFacultad(string id)
        {
            var resultado = await facultadService.GetEspeciesPorFacultadAsync(id);
            return resultado is null
                ? NotFound("Facultad no encontrada.")
                : Ok(resultado);
        }
    }
}
