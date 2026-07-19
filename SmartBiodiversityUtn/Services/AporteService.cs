using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;
using SmartBiodiversityUtnModels.DTOs.Bitacora;
using SmartBiodiversityUtnModels.Entities;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Services
{
    public class AporteService(
        SmartBiodiversityUtnContext _context, 
        IBitacoraService _bitacora,
        IHttpContextAccessor _httpContextAccessor
        ) : IAporteService
    {
        public async Task<AporteResponse> CreateAporteAsync(string idUsuario, CreateAporteRequest request)
        {
            var usuario = await _context.Usuarios.FindAsync(idUsuario);
            if (usuario == null) return null;

            var aporte = new Aporte
            {
                IdAporte = "APT-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper(),
                IdUsuarioApo = idUsuario,
                TituloApo = request.TituloApo,
                DescripcionApo = request.DescripcionApo,
                RutaArchivoApo = request.RutaArchivoApo,
                EstadoApo = EstadoAporte.Pendiente,
                FechaCreacionApo = DateTime.UtcNow
            };

            _context.Aportes.Add(aporte);
            await _context.SaveChangesAsync();

            // LOG: Aporte creado
            await _bitacora.RegistrarAccionComoAsync(
                idUsuario,
                "CREAR_APORTE",
                $"Aporte creado: '{aporte.TituloApo}' (ID: {aporte.IdAporte})");


            return MapToAporteResponse(aporte, usuario);
        }

        public async Task<AporteResponse> GetAporteByIdAsync(string idAporte)
        {
            var aporte = await _context.Aportes
                .Where(a => a.IdAporte == idAporte)
                .Select(a => new AporteResponse
                {
                    IdAporte = a.IdAporte,
                    IdUsuarioApo = a.IdUsuarioApo,
                    TituloApo = a.TituloApo,
                    DescripcionApo = a.DescripcionApo,
                    RutaArchivoApo = a.RutaArchivoApo,
                    EstadoApo = a.EstadoApo,
                    FechaCreacionApo = a.FechaCreacionApo,
                    FechaAprobacionApo = a.FechaAprobacionApo,
                    NombreUsuario = a.Usuario.Nombres + " " + a.Usuario.Apellidos,
                    CorreoUsuario = a.Usuario.Correo

                }).FirstOrDefaultAsync();




            return aporte;
        }

        public async Task<IEnumerable<AporteResponse>> GetAllAportesAsync()
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<IEnumerable<AporteResponse>> GetAportesByUsuarioAsync(string idUsuario)
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .Where(a => a.IdUsuarioApo == idUsuario)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<IEnumerable<AporteResponse>> GetAportesByEstadoAsync(EstadoAporte estado)
        {
            var aportes = await _context.Aportes
                .Include(a => a.Usuario)
                .Where(a => a.EstadoApo == estado)
                .OrderByDescending(a => a.FechaCreacionApo)
                .ToListAsync();

            return aportes.Select(a => MapToAporteResponse(a, a.Usuario)).ToList();
        }

        public async Task<bool> UpdateAporteAsync(string idAporte, UpdateAporteRequest request)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            var cambios = new List<string>();
            if (aporte.TituloApo != request.TituloApo) cambios.Add($"Título: '{aporte.TituloApo}' → '{request.TituloApo}'");
            if (aporte.DescripcionApo != request.DescripcionApo) cambios.Add($"Descripción modificada");
            if (aporte.RutaArchivoApo != request.RutaArchivoApo) cambios.Add($"Archivo actualizado");

            aporte.TituloApo = request.TituloApo;
            aporte.DescripcionApo = request.DescripcionApo;
            aporte.RutaArchivoApo = request.RutaArchivoApo;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result && cambios.Any())
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // LOG: Aporte actualizado
                await _bitacora.RegistrarAccionComoAsync(
                    currentUserId ?? "SYSTEM",
                    "ACTUALIZAR_APORTE",
                    $"Aporte actualizado (ID: {idAporte}): {string.Join("; ", cambios)}");
            }

            return result;
        }

        public async Task<bool> DeleteAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo == EstadoAporte.Aprobado) return false;

            var titulo = aporte.TituloApo;
            var autorId = aporte.IdUsuarioApo;

            _context.Aportes.Remove(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                var currentUserId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole("Administrador") == true;

                // LOG: Aporte eliminado
                await _bitacora.RegistrarAccionComoAsync(
                    currentUserId ?? "SYSTEM",
                    isAdmin ? "ELIMINAR_APORTE_ADMIN" : "ELIMINAR_PROPIO_APORTE",
                    $"Aporte eliminado: '{titulo}' (ID: {idAporte}) - Autor: {autorId}");
            }

            return result;
        }

        public async Task<bool> ApprovedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Aprobado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                // Obtener el admin actual que está aprobando
                var adminId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // LOG: Aporte aprobado
                await _bitacora.RegistrarAccionComoAsync(
                    adminId ?? "SYSTEM",
                    "APROBAR_APORTE",
                    $"Aporte aprobado: '{aporte.TituloApo}' (ID: {aporte.IdAporte}) - Usuario autor: {aporte.IdUsuarioApo}");
            }
            return result;
        }

        public async Task<bool> RejectedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Rechazado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                var adminId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                // LOG: Aporte rechazado
                await _bitacora.RegistrarAccionComoAsync(
                    adminId ?? "SYSTEM",
                    "RECHAZAR_APORTE",
                    $"Aporte rechazado: '{aporte.TituloApo}' (ID: {aporte.IdAporte}) - Usuario autor: {aporte.IdUsuarioApo}");
            }

            return result;
        }

        public async Task<int> GetCountAportesByEstadoAsync(EstadoAporte estado)
        {
            return await _context.Aportes
                .Where(a => a.EstadoApo == estado)
                .CountAsync();
        }

        private AporteResponse MapToAporteResponse(Aporte aporte, Usuario usuario)
        {
            return new AporteResponse
            {
                IdAporte = aporte.IdAporte,
                IdUsuarioApo = aporte.IdUsuarioApo,
                TituloApo = aporte.TituloApo,
                DescripcionApo = aporte.DescripcionApo,
                RutaArchivoApo = aporte.RutaArchivoApo,
                EstadoApo = aporte.EstadoApo,
                FechaCreacionApo = aporte.FechaCreacionApo,
                FechaAprobacionApo = aporte.FechaAprobacionApo,
                NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}",
                CorreoUsuario = usuario.Correo
            };
        }
    }
}
