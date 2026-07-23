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

       
    }
}
