using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartBiodiversityUtnMVC.Services;
using SmartBiodiversityUtnModels.DTOs;                 // CreateEspecieRequest
using SmartBiodiversityUtnModels.DTOs.Account;         // UserListResponse
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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var especies = (await _apiClient.GetAsync<IEnumerable<EspecieResponse>>("api/Especies")
                            ?? Enumerable.Empty<EspecieResponse>()).ToList();

            // Ids de Flora y Fauna para el selector del modal de registro rápido
            ViewBag.FloraId = await IdCategoriaAsync("Flora");
            ViewBag.FaunaId = await IdCategoriaAsync("Fauna");

            return View(especies);
        }

        [HttpGet]
        public IActionResult InventarioEspecies()
        {
            return View();
        }

        // =====================================================================
        //  USUARIOS (solo lista, trae la API con rol Administrador)
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Usuarios()
        {
            var usuarios = await _apiClient.GetAsync<IEnumerable<UserListResponse>>("api/Users/listar-todos")
                           ?? Enumerable.Empty<UserListResponse>();

            return View(usuarios.ToList());
        }

        // =====================================================================
        //  REPORTES (inventario descargable en Excel / PDF)
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Reportes()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>("api/Especies")
                           ?? Enumerable.Empty<EspecieResponse>();

            return View(especies.ToList());
        }

        // =====================================================================
        //  REGISTRO / EDICIÓN / ELIMINACIÓN desde el DASHBOARD (Index)
        //  (el usuario elige Flora o Fauna en el modal; redirige de vuelta a Index)
        // =====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EspecieCreate(CreateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(request?.NombreComun) ||
                string.IsNullOrWhiteSpace(request?.NombreCientifico) ||
                string.IsNullOrWhiteSpace(request?.CategoriaId))
            {
                TempData["IndexError"] = "Nombre común, nombre científico y categoría (Flora/Fauna) son obligatorios.";
                return RedirectToAction(nameof(Index));
            }

            // Solo permitimos Flora o Fauna desde el dashboard
            var floraId = await IdCategoriaAsync("Flora");
            var faunaId = await IdCategoriaAsync("Fauna");
            if (request.CategoriaId != floraId && request.CategoriaId != faunaId)
            {
                TempData["IndexError"] = "Categoría inválida (debe ser Flora o Fauna).";
                return RedirectToAction(nameof(Index));
            }

            var creado = await _apiClient.PostAsync<CreateEspecieRequest, EspecieResponse>("api/Especies", request);
            if (creado == null)
            {
                TempData["IndexError"] = "No se pudo registrar la especie. Verifica que tu sesión sea de Administrador.";
                return RedirectToAction(nameof(Index));
            }

            var conImagen = archivo != null && archivo.Length > 0;
            var (imgOk, imgErr) = await TrySubirImagen(creado.IdEspecie, archivo);

            TempData["IndexOk"] = imgOk
                ? $"Especie '{creado.NombreComun}' registrada" + (conImagen ? " con imagen" : "") + "."
                : $"Especie '{creado.NombreComun}' registrada, pero la imagen no se subió.";
            if (!imgOk) TempData["IndexError"] = imgErr;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EspecieEdit(string id, UpdateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["IndexError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _apiClient.PutAsync<UpdateEspecieRequest>($"api/Especies/{id}", request);
            if (!ok)
            {
                TempData["IndexError"] = "No se pudo actualizar la especie. Verifica tu sesión de Administrador.";
                return RedirectToAction(nameof(Index));
            }

            var (imgOk, imgErr) = await TrySubirImagen(id, archivo);
            TempData["IndexOk"] = imgOk
                ? "Especie actualizada" + (archivo != null && archivo.Length > 0 ? " (imagen agregada)" : "") + "."
                : "Especie actualizada, pero la imagen no se subió.";
            if (!imgOk) TempData["IndexError"] = imgErr;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EspecieDelete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["IndexError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Index));
            }

            var ok = await _apiClient.DeleteAsync($"api/Especies/{id}");
            if (ok) TempData["IndexOk"] = "Especie eliminada correctamente.";
            else TempData["IndexError"] = "No se pudo eliminar la especie. Verifica tu sesión de Administrador.";

            return RedirectToAction(nameof(Index));
        }

        // =====================================================================
        //  HELPERS
        // =====================================================================
        private async Task<string?> IdCategoriaAsync(string nombre)
        {
            var categorias = await _apiClient.GetAsync<IEnumerable<CategoriaResponse>>("api/Categorias")
                             ?? Enumerable.Empty<CategoriaResponse>();

            return categorias.FirstOrDefault(c =>
                string.Equals(c.Nombre?.Trim(), nombre, StringComparison.OrdinalIgnoreCase))?.Id;
        }

        /// <summary>
        /// Sube la imagen a la API justo después de crear/editar la especie.
        /// Devuelve (ok, error): ok=true si no había imagen o si se subió bien.
        /// NOTA: un método async NO puede usar 'out', por eso se devuelve tupla.
        /// </summary>
        private async Task<(bool ok, string? error)> TrySubirImagen(string especieId, IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return (true, null);

            if (!string.IsNullOrEmpty(archivo.ContentType) &&
                !archivo.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "El archivo debe ser una imagen (jpg, png, webp...).");
            }

            var ok = await _apiClient.PostMultipartAsync(
                "api/Multimedias",
                archivo,
                "Archivo",
                new[]
                {
                    new KeyValuePair<string, string>("EspecieId", especieId),
                    new KeyValuePair<string, string>("TipoArchivo", "Imagen")
                });

            return ok
                ? (true, null)
                : (false, "No se pudo subir la imagen (revisa formato/tamaño y que tu sesión sea de Administrador).");
        }

        // =====================================================================
        //  FLORA
        // =====================================================================
        [HttpGet]
        public async Task<IActionResult> Flora()
        {
            var especies = await _apiClient.GetAsync<IEnumerable<EspecieResponse>>("api/Especies");

            var flora = (especies ?? Enumerable.Empty<EspecieResponse>())
                .Where(e => string.Equals(e.NombreCategoria?.Trim(), "Flora", StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.FloraCategoryId = await IdCategoriaAsync("Flora");
            return View(flora);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloraCreate(CreateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(request?.NombreComun) ||
                string.IsNullOrWhiteSpace(request?.NombreCientifico))
            {
                TempData["FloraError"] = "Nombre común y nombre científico son obligatorios.";
                return RedirectToAction(nameof(Flora));
            }

            var floraId = await IdCategoriaAsync("Flora");
            if (string.IsNullOrEmpty(floraId))
            {
                TempData["FloraError"] = "No existe la categoría 'Flora' en el sistema.";
                return RedirectToAction(nameof(Flora));
            }

            request.CategoriaId = floraId;

            var creado = await _apiClient.PostAsync<CreateEspecieRequest, EspecieResponse>("api/Especies", request);
            if (creado == null)
            {
                TempData["FloraError"] = "No se pudo registrar la especie. Verifica que tu sesión sea de Administrador.";
                return RedirectToAction(nameof(Flora));
            }

            var conImagen = archivo != null && archivo.Length > 0;
            var (imgOk, imgErr) = await TrySubirImagen(creado.IdEspecie, archivo);

            TempData["FloraOk"] = imgOk
                ? $"Especie '{creado.NombreComun}' registrada" + (conImagen ? " con imagen" : "") + "."
                : $"Especie '{creado.NombreComun}' registrada, pero la imagen no se subió.";

            if (!imgOk) TempData["FloraError"] = imgErr;

            return RedirectToAction(nameof(Flora));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FloraEdit(string id, UpdateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FloraError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Flora));
            }

            var ok = await _apiClient.PutAsync<UpdateEspecieRequest>($"api/Especies/{id}", request);
            if (!ok)
            {
                TempData["FloraError"] = "No se pudo actualizar la especie. Verifica tu sesión de Administrador.";
                return RedirectToAction(nameof(Flora));
            }

            var (imgOk, imgErr) = await TrySubirImagen(id, archivo);
            TempData["FloraOk"] = imgOk
                ? "Especie actualizada" + (archivo != null && archivo.Length > 0 ? " (imagen agregada)" : "") + "."
                : "Especie actualizada, pero la imagen no se subió.";
            if (!imgOk) TempData["FloraError"] = imgErr;

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
                .Where(e => string.Equals(e.NombreCategoria?.Trim(), "Fauna", StringComparison.OrdinalIgnoreCase))
                .ToList();

            ViewBag.FaunaCategoryId = await IdCategoriaAsync("Fauna");
            return View(fauna);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FaunaCreate(CreateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(request?.NombreComun) ||
                string.IsNullOrWhiteSpace(request?.NombreCientifico))
            {
                TempData["FaunaError"] = "Nombre común y nombre científico son obligatorios.";
                return RedirectToAction(nameof(Fauna));
            }

            var faunaId = await IdCategoriaAsync("Fauna");
            if (string.IsNullOrEmpty(faunaId))
            {
                TempData["FaunaError"] = "No existe la categoría 'Fauna' en el sistema.";
                return RedirectToAction(nameof(Fauna));
            }

            request.CategoriaId = faunaId;

            var creado = await _apiClient.PostAsync<CreateEspecieRequest, EspecieResponse>("api/Especies", request);
            if (creado == null)
            {
                TempData["FaunaError"] = "No se pudo registrar la especie. Verifica que tu sesión sea de Administrador.";
                return RedirectToAction(nameof(Fauna));
            }

            var conImagen = archivo != null && archivo.Length > 0;
            var (imgOk, imgErr) = await TrySubirImagen(creado.IdEspecie, archivo);

            TempData["FaunaOk"] = imgOk
                ? $"Especie '{creado.NombreComun}' registrada" + (conImagen ? " con imagen" : "") + "."
                : $"Especie '{creado.NombreComun}' registrada, pero la imagen no se subió.";

            if (!imgOk) TempData["FaunaError"] = imgErr;

            return RedirectToAction(nameof(Fauna));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FaunaEdit(string id, UpdateEspecieRequest request, IFormFile? archivo)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["FaunaError"] = "ID de especie inválido.";
                return RedirectToAction(nameof(Fauna));
            }

            var ok = await _apiClient.PutAsync<UpdateEspecieRequest>($"api/Especies/{id}", request);
            if (!ok)
            {
                TempData["FaunaError"] = "No se pudo actualizar la especie. Verifica tu sesión de Administrador.";
                return RedirectToAction(nameof(Fauna));
            }

            var (imgOk, imgErr) = await TrySubirImagen(id, archivo);
            TempData["FaunaOk"] = imgOk
                ? "Especie actualizada" + (archivo != null && archivo.Length > 0 ? " (imagen agregada)" : "") + "."
                : "Especie actualizada, pero la imagen no se subió.";
            if (!imgOk) TempData["FaunaError"] = imgErr;

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
