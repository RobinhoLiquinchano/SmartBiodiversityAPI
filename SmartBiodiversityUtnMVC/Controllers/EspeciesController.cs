using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtnMVC.Services;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;
using SmartBiodiversityUtnModels.DTOs.Especie;

namespace SmartBiodiversityUtnMVC.Controllers
{
    public class EspeciesController : Controller
    {
        private readonly ApiClientService _apiClient;

        public EspeciesController(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>(
                "api/Especies");

            return View(especies ?? Enumerable.Empty<EspecieResponse>());
        }

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            var especie = await _apiClient.GetAsync<EspecieResponse>(
                $"api/Especies/{Uri.EscapeDataString(id)}");

            if (especie == null)
                return NotFound();

            return View(especie);
        }

        // Página pública: solo recibe aportes que ya fueron aprobados por un administrador.
        [HttpGet]
        public async Task<IActionResult> AportesComunidad()
        {
            var aportes = await _apiClient.GetAsync<IEnumerable<AporteResponse>>(
                "api/Aportes/publicos");

            var aprobados = (aportes ?? Enumerable.Empty<AporteResponse>())
                .Where(a => a.EstadoApo == EstadoAporte.Aprobado)
                .OrderByDescending(a => a.FechaAprobacionApo ?? a.FechaCreacionApo)
                .ToList();

            return View(aprobados);
        }
    }
}
