using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.DTOs.Multimedia;

namespace SmartBiodiversityUtn.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MultimediasController(IMultimediaService multimediaService) : ControllerBase
    {


        [HttpGet]
        public async Task<ActionResult<IEnumerable<MultimediaResponse>>> GetAll()
        {
            var result = await multimediaService.GetMultimediaByEspecieIdAsync();
            return Ok(result);
        }

        [HttpGet("{especieId}")]
        public async Task<ActionResult<MultimediaResponse>> GetByEspecie(string especieId)
        {
            var result = await multimediaService.GetMultimediaByEspecieIdAsync(especieId);

            if (result == null)
                return NotFound(new { message = $"No se encontró multimedia para la especie {especieId}" });

            return Ok(result);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<MultimediaResponse>> Upload([FromForm] CreateMultimediaRequest request)
        {
            try
            {
                var result = await multimediaService.AddMultimediaAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await multimediaService.DeleteMultimediaAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
