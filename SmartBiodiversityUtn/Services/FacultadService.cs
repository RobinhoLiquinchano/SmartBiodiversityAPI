using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Facultad;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class FacultadService : IFacultadService
    {
        private readonly SmartBiodiversityUtnContext _context;

        public FacultadService(SmartBiodiversityUtnContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FacultadResponse>> GetAllFacultadesAsync()
        {
            var facultades = await _context.Facultades
                .Include(f => f.EspecieFacultades)
                .ToListAsync();

            return facultades.Select(f => new FacultadResponse
            {
                IdFacultad = f.IdFacultad,
                Nombre = f.NombreFac,
                Latitud = f.Latitud,
                Longitud = f.Longitud,
                Descripcion = f.DescripcionFac,
                TotalEspecies = f.EspecieFacultades.Count
            });
        }

        /// <summary>
        /// Devuelve una facultad específica por su ID.
        /// </summary>
        public async Task<FacultadResponse?> GetFacultadByIdAsync(string id)
        {
            var f = await _context.Facultades
                .Include(fac => fac.EspecieFacultades)
                .FirstOrDefaultAsync(fac => fac.IdFacultad == id);

            if (f == null) return null;

            return new FacultadResponse
            {
                IdFacultad = f.IdFacultad,
                Nombre = f.NombreFac,
                Latitud = f.Latitud,
                Longitud = f.Longitud,
                Descripcion = f.DescripcionFac,
                TotalEspecies = f.EspecieFacultades.Count
            };
        }

        /// <summary>
        /// Devuelve las especies de una facultad, separadas en Flora y Fauna,
        /// con sus conteos. Se usa para el panel lateral del mapa interactivo.
        /// </summary>
        public async Task<FacultadEspeciesResponse?> GetEspeciesPorFacultadAsync(string idFacultad)
        {
            var facultad = await _context.Facultades
                .FirstOrDefaultAsync(f => f.IdFacultad == idFacultad);

            if (facultad == null) return null;

            // Consulta única: trae todas las especies de esta facultad
            // con su categoría y la URL de imagen más reciente.
            var especiesRaw = await _context.EspecieFacultades
                .Where(ef => ef.IdFacultad == idFacultad)
                .Include(ef => ef.Especie)
                    .ThenInclude(e => e.Categoria)
                .Select(ef => new
                {
                    ef.Especie.IdEspecies,
                    ef.Especie.NombreComunEsp,
                    ef.Especie.NombreCientificoEsp,
                    CategoriaNombre = ef.Especie.Categoria.NombreCat,
                    // Sub-consulta: la imagen más reciente de esta especie
                    ImagenUrl = ef.Especie.MultimediaArchivos
                        .Where(m => !string.IsNullOrEmpty(m.RutaArchivoMul))
                        .OrderByDescending(m => m.FechaMul)
                        .Select(m => m.RutaArchivoMul)
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Separar en Flora y Fauna según el nombre de la categoría
            var flora = especiesRaw
                .Where(e => string.Equals(e.CategoriaNombre?.Trim(), "Flora", StringComparison.OrdinalIgnoreCase))
                .Select(e => new EspecieResumenDto
                {
                    IdEspecie = e.IdEspecies,
                    NombreComun = e.NombreComunEsp,
                    NombreCientifico = e.NombreCientificoEsp,
                    Categoria = e.CategoriaNombre ?? "Sin categoría",
                    ImagenUrl = e.ImagenUrl
                })
                .ToList();

            var fauna = especiesRaw
                .Where(e => string.Equals(e.CategoriaNombre?.Trim(), "Fauna", StringComparison.OrdinalIgnoreCase))
                .Select(e => new EspecieResumenDto
                {
                    IdEspecie = e.IdEspecies,
                    NombreComun = e.NombreComunEsp,
                    NombreCientifico = e.NombreCientificoEsp,
                    Categoria = e.CategoriaNombre ?? "Sin categoría",
                    ImagenUrl = e.ImagenUrl
                })
                .ToList();

            return new FacultadEspeciesResponse
            {
                IdFacultad = facultad.IdFacultad,
                NombreFacultad = facultad.NombreFac,
                Latitud = facultad.Latitud,
                Longitud = facultad.Longitud,
                Flora = flora,
                Fauna = fauna,
                TotalFlora = flora.Count,
                TotalFauna = fauna.Count,
                TotalGeneral = flora.Count + fauna.Count
            };
        }
    }
}
