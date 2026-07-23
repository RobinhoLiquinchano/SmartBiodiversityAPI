using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtn.Helpers;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Especie;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class EspecieService : IEspecieService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IBitacoraService _bitacoraService;

        public EspecieService(SmartBiodiversityUtnContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie, string idUsuario)
        {
            // ====== VALIDACIÓN: No permitir especies duplicadas por nombre científico ======
            var existeDuplicado = await _context.Especies
                .AnyAsync(e => e.NombreCientificoEsp.ToLower().Trim() == especie.NombreCientifico.ToLower().Trim());

            if (existeDuplicado)
            {
                throw new InvalidOperationException(
                    $"Ya existe una especie registrada con el nombre científico '{especie.NombreCientifico}'.");
            }

            var newEspecie = new Especie
            {
                IdEspecies = "ESP-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                IdCategoriaEsp = especie.CategoriaId,
                NombreComunEsp = especie.NombreComun,
                NombreCientificoEsp = especie.NombreCientifico,
                DescripcionEsp = especie.Descripcion,
                HabitatEsp = especie.Habitat,
                EstadoEsp = EstadoEspecie.Activo,
                FechaRegistroEsp = DateTime.UtcNow
            };

            _context.Especies.Add(newEspecie);
            await _context.SaveChangesAsync();

            // ====== NUEVO: Crear detalle opcional (Flora / Fauna) si viene en la petición ======
            if (especie.DetalleFlora != null)
            {
                _context.DetallesFlora.Add(MapFloraFromDto(newEspecie.IdEspecies, especie.DetalleFlora));
                await _context.SaveChangesAsync();
            }
            if (especie.DetalleFauna != null)
            {
                _context.DetallesFauna.Add(MapFaunaFromDto(newEspecie.IdEspecies, especie.DetalleFauna));
                await _context.SaveChangesAsync();
            }

            // ====== Vincular la especie con la facultad si se proporciona ======
            string? nombreFacultad = null;
            string? idFacultad = null;

            if (!string.IsNullOrWhiteSpace(especie.FacultadId))
            {
                var facultad = await _context.Facultades.FindAsync(especie.FacultadId);
                if (facultad != null)
                {
                    var enlace = new EspecieFacultad
                    {
                        IdEspecieFacultad = "EF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                        IdEspecies = newEspecie.IdEspecies,
                        IdFacultad = especie.FacultadId,
                        FechaAsignacion = DateTime.UtcNow
                    };
                    _context.EspecieFacultades.Add(enlace);
                    await _context.SaveChangesAsync();

                    nombreFacultad = facultad.NombreFac;
                    idFacultad = facultad.IdFacultad;
                }
            }

            var categoria = await _context.Categorias.FindAsync(newEspecie.IdCategoriaEsp);

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "CREAR_ESPECIE",
                $"Creó la especie: {especie.NombreComun}" +
                (nombreFacultad != null ? $" en {nombreFacultad}" : ""));

            // En la respuesta de creación incluimos el detalle recién creado (si lo hubo).
            var resp = MapToResponse(newEspecie, categoria?.NombreCat ?? "Sin categoría", null, nombreFacultad, idFacultad);
            resp.DetalleFlora = especie.DetalleFlora;
            resp.DetalleFauna = especie.DetalleFauna;
            return resp;
        }

        public async Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie, string idUsuario)
        {
            var existing = await _context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (existing == null) return false;

            // ====== VALIDACIÓN: Si cambia el nombre científico, verificar que no exista duplicado ======
            if (!string.IsNullOrWhiteSpace(especie.NombreCientifico) &&
                especie.NombreCientifico.ToLower().Trim() != existing.NombreCientificoEsp.ToLower().Trim())
            {
                var existeDuplicado = await _context.Especies
                    .AnyAsync(e => e.IdEspecies != id &&
                                   e.NombreCientificoEsp.ToLower().Trim() == especie.NombreCientifico.ToLower().Trim());

                if (existeDuplicado)
                {
                    throw new InvalidOperationException(
                        $"Ya existe otra especie registrada con el nombre científico '{especie.NombreCientifico}'.");
                }
            }

            // Actualizar campos básicos
            existing.NombreComunEsp = especie.NombreComun ?? existing.NombreComunEsp;
            existing.NombreCientificoEsp = especie.NombreCientifico ?? existing.NombreCientificoEsp;
            existing.DescripcionEsp = especie.Descripcion ?? existing.DescripcionEsp;
            existing.HabitatEsp = especie.Habitat ?? existing.HabitatEsp;
            existing.EstadoEsp = especie.EstadoEsp ?? existing.EstadoEsp;
            existing.IdCategoriaEsp = especie.IdCategoria ?? existing.IdCategoriaEsp;

            // ====== Manejar cambio de facultad ======
            if (especie.FacultadId != null) // Se envió el campo (puede ser vacío para desvincular)
            {
                // Buscar enlace actual
                var enlaceActual = await _context.EspecieFacultades
                    .FirstOrDefaultAsync(ef => ef.IdEspecies == id);

                if (string.IsNullOrWhiteSpace(especie.FacultadId))
                {
                    // FacultadId vacío → desvincular
                    if (enlaceActual != null)
                    {
                        _context.EspecieFacultades.Remove(enlaceActual);
                    }
                }
                else
                {
                    // FacultadId con valor → vincular o cambiar
                    var facultad = await _context.Facultades.FindAsync(especie.FacultadId);
                    if (facultad != null)
                    {
                        if (enlaceActual == null)
                        {
                            // No había enlace → crear nuevo
                            var nuevoEnlace = new EspecieFacultad
                            {
                                IdEspecieFacultad = "EF-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                                IdEspecies = id,
                                IdFacultad = especie.FacultadId,
                                FechaAsignacion = DateTime.UtcNow
                            };
                            _context.EspecieFacultades.Add(nuevoEnlace);
                        }
                        else if (enlaceActual.IdFacultad != especie.FacultadId)
                        {
                            // Ya había enlace pero diferente facultad → actualizar
                            enlaceActual.IdFacultad = especie.FacultadId;
                            enlaceActual.FechaAsignacion = DateTime.UtcNow;
                        }
                        // Si es la misma facultad, no hacer nada
                    }
                }
            }

            // ====== NUEVO: Upsert del detalle Flora (solo si viene en la petición) ======
            if (especie.DetalleFlora != null)
            {
                var floraActual = await _context.DetallesFlora.FirstOrDefaultAsync(d => d.IdEspecies == id);
                if (floraActual == null)
                {
                    _context.DetallesFlora.Add(MapFloraFromDto(id, especie.DetalleFlora));
                }
                else
                {
                    CopyFloraFromDto(floraActual, especie.DetalleFlora);
                }
            }

            // ====== NUEVO: Upsert del detalle Fauna (solo si viene en la petición) ======
            if (especie.DetalleFauna != null)
            {
                var faunaActual = await _context.DetallesFauna.FirstOrDefaultAsync(d => d.IdEspecies == id);
                if (faunaActual == null)
                {
                    _context.DetallesFauna.Add(MapFaunaFromDto(id, especie.DetalleFauna));
                }
                else
                {
                    CopyFaunaFromDto(faunaActual, especie.DetalleFauna);
                }
            }

            _context.Especies.Update(existing);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "EDITAR_ESPECIE",
                    $"Editó la especie: {existing.NombreComunEsp}");
            }

            return result;
        }

        public async Task<bool> DeleteEspecieAsync(string id, string idUsuario)
        {
            var especie = await _context.Especies.FirstOrDefaultAsync(e => e.IdEspecies == id);
            if (especie == null) return false;

            // Eliminar también las relaciones con facultades
            var enlaces = await _context.EspecieFacultades
                .Where(ef => ef.IdEspecies == id)
                .ToListAsync();
            if (enlaces.Count > 0)
                _context.EspecieFacultades.RemoveRange(enlaces);

            // El detalle Flora/Fauna se elimina en cascada por la FK,
            // pero lo removemos explícitamente por claridad y compatibilidad.
            var flora = await _context.DetallesFlora.FirstOrDefaultAsync(d => d.IdEspecies == id);
            if (flora != null) _context.DetallesFlora.Remove(flora);

            var fauna = await _context.DetallesFauna.FirstOrDefaultAsync(d => d.IdEspecies == id);
            if (fauna != null) _context.DetallesFauna.Remove(fauna);

            var nombre = especie.NombreComunEsp;
            _context.Especies.Remove(especie);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "ELIMINAR_ESPECIE",
                    $"Eliminó la especie: {nombre}");
            }

            return result;
        }

        // ==================== Sobrecargas antiguas (compatibilidad) ====================
        public async Task<EspecieResponse> AddEspecieAsync(CreateEspecieRequest especie)
            => await AddEspecieAsync(especie, "SYSTEM");

        public async Task<bool> UpdateEspecieAsync(string id, UpdateEspecieRequest especie)
            => await UpdateEspecieAsync(id, especie, "SYSTEM");

        public async Task<bool> DeleteEspecieAsync(string id)
            => await DeleteEspecieAsync(id, "SYSTEM");

        // ==================== Métodos de lectura ====================
        public async Task<IEnumerable<EspecieResponse>> GetAllEspeciesAsync()
        {
            // IMPORTANTE: el listado NO incluye ni expone el detalle ampliado.
            // Se mantiene idéntico al comportamiento previo para no afectar a
            // los consumidores del controlador de especies.
            var especies = await _context.Especies
                .Include(e => e.Categoria)
                .Include(e => e.EspecieFacultades)
                    .ThenInclude(ef => ef.Facultad)
                .ToListAsync();

            var dictImagenes = await MapaImagenesPorEspecieAsync();

            return especies
                .Select(e =>
                {
                    var primeraFac = e.EspecieFacultades.FirstOrDefault();
                    return MapToResponse(
                        e,
                        e.Categoria?.NombreCat ?? "Sin categoría",
                        dictImagenes.TryGetValue(e.IdEspecies, out var url) ? url : null,
                        primeraFac?.Facultad?.NombreFac,
                        primeraFac?.IdFacultad);
                })
                .ToList();
        }

        public async Task<EspecieResponse?> GetEspecieByIdAsync(string id)
        {
            var especie = await _context.Especies
                .Include(e => e.Categoria)
                .Include(e => e.EspecieFacultades)
                    .ThenInclude(ef => ef.Facultad)
                .Include(e => e.DetalleFlora)   // NUEVO: solo en el detalle
                .Include(e => e.DetalleFauna)   // NUEVO: solo en el detalle
                .FirstOrDefaultAsync(e => e.IdEspecies == id);

            if (especie == null) return null;

            var url = await _context.Multimedia
                .Where(m => m.IdEspeciesMul == id && !string.IsNullOrEmpty(m.RutaArchivoMul))
                .OrderByDescending(m => m.FechaMul)
                .Select(m => m.RutaArchivoMul)
                .FirstOrDefaultAsync();

            var primeraFac = especie.EspecieFacultades.FirstOrDefault();

            var response = MapToResponse(
                especie,
                especie.Categoria?.NombreCat ?? "Sin categoría",
                url,
                primeraFac?.Facultad?.NombreFac,
                primeraFac?.IdFacultad);

            // NUEVO: adjuntar el detalle ampliado (será null si no existe).
            response.DetalleFlora = MapFloraToDto(especie.DetalleFlora);
            response.DetalleFauna = MapFaunaToDto(especie.DetalleFauna);

            return response;
        }

        private async Task<Dictionary<string, string>> MapaImagenesPorEspecieAsync()
        {
            var multimedias = await _context.Multimedia
                .Where(m => !string.IsNullOrEmpty(m.RutaArchivoMul))
                .ToListAsync();

            return multimedias
                .GroupBy(m => m.IdEspeciesMul)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(m => m.FechaMul).First().RutaArchivoMul);
        }

        private static EspecieResponse MapToResponse(
            Especie especie,
            string nombreCategoria,
            string? imagenUrl,
            string? nombreFacultad = null,
            string? idFacultad = null)
        {
            return new EspecieResponse
            {
                IdEspecie = especie.IdEspecies,
                NombreComun = especie.NombreComunEsp,
                NombreCientifico = especie.NombreCientificoEsp,
                Descripcion = especie.DescripcionEsp,
                Habitat = especie.HabitatEsp,
                EstadoEsp = especie.EstadoEsp,
                NombreCategoria = nombreCategoria,
                FechaRegistroEsp = especie.FechaRegistroEsp.ToEcuadorTime(),
                ImagenUrl = imagenUrl,
                NombreFacultad = nombreFacultad,
                IdFacultad = idFacultad
                // DetalleFlora / DetalleFauna quedan en null salvo que se rellenen aparte.
            };
        }

        // ==================== Mapeos de detalle (Entidad <-> DTO) ====================

        private static DetalleFlora MapFloraFromDto(string idEspecie, DetalleFloraDto dto)
        {
            var entidad = new DetalleFlora { IdEspecies = idEspecie };
            CopyFloraFromDto(entidad, dto);
            return entidad;
        }

        private static void CopyFloraFromDto(DetalleFlora entidad, DetalleFloraDto dto)
        {
            entidad.AlturaPromedioM = dto.AlturaPromedioM;
            entidad.AlturaMaximaM = dto.AlturaMaximaM;
            entidad.DiametroTroncoCm = dto.DiametroTroncoCm;
            entidad.TipoCortezaTronco = dto.TipoCortezaTronco;
            entidad.FormaCopa = dto.FormaCopa;
            entidad.TipoHoja = dto.TipoHoja;
            entidad.ColorFlorFruto = dto.ColorFlorFruto;
            entidad.HabitoCrecimiento = dto.HabitoCrecimiento;
        }

        private static DetalleFloraDto? MapFloraToDto(DetalleFlora? entidad)
        {
            if (entidad == null) return null;
            return new DetalleFloraDto
            {
                AlturaPromedioM = entidad.AlturaPromedioM,
                AlturaMaximaM = entidad.AlturaMaximaM,
                DiametroTroncoCm = entidad.DiametroTroncoCm,
                TipoCortezaTronco = entidad.TipoCortezaTronco,
                FormaCopa = entidad.FormaCopa,
                TipoHoja = entidad.TipoHoja,
                ColorFlorFruto = entidad.ColorFlorFruto,
                HabitoCrecimiento = entidad.HabitoCrecimiento
            };
        }

        private static DetalleFauna MapFaunaFromDto(string idEspecie, DetalleFaunaDto dto)
        {
            var entidad = new DetalleFauna { IdEspecies = idEspecie };
            CopyFaunaFromDto(entidad, dto);
            return entidad;
        }

        private static void CopyFaunaFromDto(DetalleFauna entidad, DetalleFaunaDto dto)
        {
            entidad.LongitudPromedioCm = dto.LongitudPromedioCm;
            entidad.EnvergaduraCm = dto.EnvergaduraCm;
            entidad.PesoPromedioGramos = dto.PesoPromedioGramos;
            entidad.TipoPelajePlumaje = dto.TipoPelajePlumaje;
            entidad.DimorfismoSexual = dto.DimorfismoSexual;
            entidad.Dieta = dto.Dieta;
            entidad.PatronActividad = dto.PatronActividad;
        }

        private static DetalleFaunaDto? MapFaunaToDto(DetalleFauna? entidad)
        {
            if (entidad == null) return null;
            return new DetalleFaunaDto
            {
                LongitudPromedioCm = entidad.LongitudPromedioCm,
                EnvergaduraCm = entidad.EnvergaduraCm,
                PesoPromedioGramos = entidad.PesoPromedioGramos,
                TipoPelajePlumaje = entidad.TipoPelajePlumaje,
                DimorfismoSexual = entidad.DimorfismoSexual,
                Dieta = entidad.Dieta,
                PatronActividad = entidad.PatronActividad
            };
        }
    }
}
