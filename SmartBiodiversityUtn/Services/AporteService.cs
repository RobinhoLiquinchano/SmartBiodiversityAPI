using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs;
using SmartBiodiversityUtnModels.DTOs.Aporte;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class AporteService(SmartBiodiversityUtnContext _context) : IAporteService
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

            aporte.TituloApo = request.TituloApo;
            aporte.DescripcionApo = request.DescripcionApo;
            aporte.RutaArchivoApo = request.RutaArchivoApo;

            _context.Aportes.Update(aporte);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo == EstadoAporte.Aprobado) return false;

            _context.Aportes.Remove(aporte);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ApprovedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Aprobado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectedAporteAsync(string idAporte)
        {
            var aporte = await _context.Aportes.FindAsync(idAporte);
            if (aporte == null || aporte.EstadoApo != EstadoAporte.Pendiente) return false;

            aporte.EstadoApo = EstadoAporte.Rechazado;
            aporte.FechaAprobacionApo = DateTime.UtcNow;

            _context.Aportes.Update(aporte);
            return await _context.SaveChangesAsync() > 0;
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
