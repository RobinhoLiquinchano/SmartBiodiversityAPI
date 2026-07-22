using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtnMVC.Services;
using SmartBiodiversityUtnModels.DTOs;                 // CreateEspecieRequest
using SmartBiodiversityUtnModels.DTOs.Categoria;       // CategoriaResponse
using SmartBiodiversityUtnModels.DTOs.Especie;         // EspecieResponse, UpdateEspecieRequest

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

        // =====================================================================
        //  FLORA
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Flora()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>("api/Especies");

            var flora = (especies ?? Enumerable.Empty<EspecieResponse>())
                .Where(e => string.Equals(
                    e.NombreCategoria?.Trim(), "Flora", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Id de la categoría "Flora" para el formulario de registro
            var categorias = await _apiClient.GetAsync<IEnumerable<CategoriaResponse>>("api/Categorias")
                             ?? Enumerable.Empty<CategoriaResponse>();

            var floraCat = categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre?.Trim(), "Flora", StringComparison.OrdinalIgnoreCase));

            ViewBag.FloraCategoryId = floraCat?.Id;

            return View(flora);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloraCreate(CreateEspecieRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.NombreComun) ||
                string.IsNullOrWhiteSpace(request?.NombreCientifico))
            {
                TempData["FloraError"] = "Nombre común y nombre científico son obligatorios.";
                return RedirectToAction(nameof(Flora));
            }

            var categorias = await _apiClient.GetAsync<IEnumerable<CategoriaResponse>>("api/Categorias")
                             ?? Enumerable.Empty<CategoriaResponse>();

            var floraId = categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre?.Trim(), "Flora", StringComparison.OrdinalIgnoreCase))?.Id;

            if (string.IsNullOrEmpty(floraId))
            {
                TempData["FloraError"] = "No existe la categoría 'Flora' en el sistema.";
                return RedirectToAction(nameof(Flora));
            }

            request.CategoriaId = floraId;

            var creado = await _apiClient.PostAsync<CreateEspecieRequest, EspecieResponse>("api/Especies", request);

            if (creado == null)
                TempData["FloraError"] = "No se pudo registrar la especie. Verifica que tu sesión sea de Administrador.";
            else
                TempData["FloraOk"] = $"Especie '{creado.NombreComun}' registrada correctamente.";

            return RedirectToAction(nameof(Flora));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloraEdit(string id, UpdateEspecieRequest request)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FloraError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Flora));
            }

            var ok = await _apiClient.PutAsync<UpdateEspecieRequest>($"api/Especies/{id}", request);

            if (ok) TempData["FloraOk"] = "Especie actualizada correctamente.";
            else TempData["FloraError"] = "No se pudo actualizar la especie. Verifica tu sesión de Administrador.";

            return RedirectToAction(nameof(Flora));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloraDelete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FloraError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Flora));
            }

            var ok = await _apiClient.DeleteAsync($"api/Especies/{id}");

            if (ok) TempData["FloraOk"] = "Especie eliminada correctamente.";
            else TempData["FloraError"] = "No se pudo eliminar la especie. Verifica tu sesión de Administrador.";

            return RedirectToAction(nameof(Flora));
        }

        // =====================================================================
        //  FAUNA
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Fauna()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>("api/Especies");

            var fauna = (especies ?? Enumerable.Empty<EspecieResponse>())
                .Where(e => string.Equals(
                    e.NombreCategoria?.Trim(), "Fauna", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var categorias = await _apiClient.GetAsync<IEnumerable<CategoriaResponse>>("api/Categorias")
                             ?? Enumerable.Empty<CategoriaResponse>();

            var faunaCat = categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre?.Trim(), "Fauna", StringComparison.OrdinalIgnoreCase));

            ViewBag.FaunaCategoryId = faunaCat?.Id;

            return View(fauna);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FaunaCreate(CreateEspecieRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.NombreComun) ||
                string.IsNullOrWhiteSpace(request?.NombreCientifico))
            {
                TempData["FaunaError"] = "Nombre común y nombre científico son obligatorios.";
                return RedirectToAction(nameof(Fauna));
            }

            var categorias = await _apiClient.GetAsync<IEnumerable<CategoriaResponse>>("api/Categorias")
                             ?? Enumerable.Empty<CategoriaResponse>();

            var faunaId = categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre?.Trim(), "Fauna", StringComparison.OrdinalIgnoreCase))?.Id;

            if (string.IsNullOrEmpty(faunaId))
            {
                TempData["FaunaError"] = "No existe la categoría 'Fauna' en el sistema.";
                return RedirectToAction(nameof(Fauna));
            }

            request.CategoriaId = faunaId;

            var creado = await _apiClient.PostAsync<CreateEspecieRequest, EspecieResponse>("api/Especies", request);

            if (creado == null)
                TempData["FaunaError"] = "No se pudo registrar la especie. Verifica que tu sesión sea de Administrador.";
            else
                TempData["FaunaOk"] = $"Especie '{creado.NombreComun}' registrada correctamente.";

            return RedirectToAction(nameof(Fauna));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FaunaEdit(string id, UpdateEspecieRequest request)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FaunaError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Fauna));
            }

            var ok = await _apiClient.PutAsync<UpdateEspecieRequest>($"api/Especies/{id}", request);

            if (ok) TempData["FaunaOk"] = "Especie actualizada correctamente.";
            else TempData["FaunaError"] = "No se pudo actualizar la especie. Verifica tu sesión de Administrador.";

            return RedirectToAction(nameof(Fauna));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FaunaDelete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FaunaError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Fauna));
            }

            var ok = await _apiClient.DeleteAsync($"api/Especies/{id}");

            if (ok) TempData["FaunaOk"] = "Especie eliminada correctamente.";
            else TempData["FaunaError"] = "No se pudo eliminar la especie. Verifica tu sesión de Administrador.";

            return RedirectToAction(nameof(Fauna));
        }
    }
}
