using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtnMVC.Services;
using SmartBiodiversityUtnModels.DTOs.Especie;

namespace SmartBiodiversityUtnMVC.Controllers
{
    public class AdministracionController : Controller
    {
        private readonly ApiClientService _apiClient;

        public AdministracionController(ApiClientService apiClient)
        {
            _apiClient = apiClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult InventarioEspecies()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Flora()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>(
                "api/Especies"
            );

            var flora = (especies ?? Enumerable.Empty<EspecieResponse>())
                .Where(e => string.Equals(
                    e.NombreCategoria?.Trim(),
                    "Flora",
                    StringComparison.OrdinalIgnoreCase
                ))
                .ToList();

            return View(flora);
        }

        [HttpGet]
        public async Task<IActionResult> Fauna()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>(
                "api/Especies"
            );

            var fauna = (especies ?? Enumerable.Empty<EspecieResponse>())
                .Where(e => string.Equals(
                    e.NombreCategoria?.Trim(),
                    "Fauna",
                    StringComparison.OrdinalIgnoreCase
                ))
                .ToList();

            return View(fauna);
        }
    }
}