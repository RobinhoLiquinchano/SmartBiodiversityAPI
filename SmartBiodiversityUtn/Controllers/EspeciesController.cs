using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspeciesController(IEspecieService especieService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EspecieResponse>>> GetAllEspecies()
        => Ok(await especieService.GetAllEspeciesAsync());

        [HttpGet("{id}")]
        public async Task<ActionResult<EspecieResponse>> GetEspecieById(string id)
        {
            var especieResponse = await especieService.GetEspecieByIdAsync(id);
            return especieResponse is null ? NotFound("No se encontró la especie con el ID proporcionado.") : Ok(especieResponse);
        }

        [HttpPost]
        public async Task<ActionResult<EspecieResponse>> AddEspecie(CreateEspecieRequest especie)
        {
            var createdEspecie = await especieService.AddEspecieAsync(especie);
            return CreatedAtAction(nameof(GetEspecieById), new { id = createdEspecie.IdEspecie }, createdEspecie);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateEspecie(string id, UpdateEspecieRequest especie)
        {
            var updated = await especieService.UpdateEspecieAsync(id, especie);
            return updated ? NoContent() : NotFound("No se encontró la especie con el ID proporcionado.");
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteEspecie(string id)
        {
            var deleted = await especieService.DeleteEspecieAsync(id);
            return deleted ? NoContent() : NotFound("No se encontró la especie con el ID proporcionado.");
        }
    }
}
