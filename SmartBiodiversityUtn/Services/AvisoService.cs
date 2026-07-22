using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtn.Helpers;
using SmartBiodiversityUtnModels.DTOs.Aviso;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class AvisoService : IAvisoService
    {
        private readonly SmartBiodiversityUtnContext _context;
        private readonly IBitacoraService _bitacoraService;

        public AvisoService(SmartBiodiversityUtnContext context, IBitacoraService bitacoraService)
        {
            _context = context;
            _bitacoraService = bitacoraService;
        }

        public async Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request, string idUsuario)
        {
            var adminRol = await _context.Roles.FirstOrDefaultAsync(r => r.NombreRol == "Administrador");
            if (adminRol == null) throw new Exception("Rol Administrador no encontrado.");

            var aviso = new Aviso
            {
                IdAvisos = "AVI-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                IdRolesAvi = adminRol.IdRoles,
                TituloAvi = request.TituloAvi,
                MensajeAvi = request.MensajeAvi,
                CategoriaAvi = request.CategoriaAvi,
                ActivoAvi = true,
                FechaIniAvi = DateTime.UtcNow,
                FechaFinAvi = request.FechaFinAvi.ToUtc()
            };

            _context.Avisos.Add(aviso);
            await _context.SaveChangesAsync();

            await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "CREAR_AVISO",
                $"Creó el aviso: {request.TituloAvi}");

            return await MapToResponse(aviso);
        }

        public async Task<bool> DeleteAvisoAsync(string id, string idUsuario)
        {
            var aviso = await _context.Avisos.FindAsync(id);
            if (aviso is null) return false;

            var titulo = aviso.TituloAvi;
            _context.Avisos.Remove(aviso);
            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "ELIMINAR_AVISO",
                    $"Eliminó el aviso: {titulo}");
            }

            return result;
        }

        public async Task<IEnumerable<AvisoResponse>> GetAllAvisosAsync()
        {
            var avisos = await _context.Avisos.Include(a => a.Rol).ToListAsync();
            var list = new List<AvisoResponse>();
            foreach (var a in avisos) list.Add(await MapToResponse(a));
            return list;
        }

        public async Task<AvisoResponse?> GetAvisoByIdAsync(string id)
        {
            var aviso = await _context.Avisos.Include(a => a.Rol).FirstOrDefaultAsync(a => a.IdAvisos == id);
            return aviso is null ? null : await MapToResponse(aviso);
        }

        public async Task<IEnumerable<AvisoResponse>> GetAvisosActivosAsync()
        {
            var hoy = DateTime.UtcNow;
            var avisos = await _context.Avisos
                .Include(a => a.Rol)
                .Where(a => a.ActivoAvi && a.FechaIniAvi <= hoy && (a.FechaFinAvi == null || a.FechaFinAvi >= hoy))
                .ToListAsync();

            var list = new List<AvisoResponse>();
            foreach (var a in avisos) list.Add(await MapToResponse(a));
            return list;
        }

        public async Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request, string idUsuario)
        {
            var aviso = await _context.Avisos.FindAsync(id);
            if (aviso is null) return false;

            if (request.TituloAvi != null) aviso.TituloAvi = request.TituloAvi;
            if (request.MensajeAvi != null) aviso.MensajeAvi = request.MensajeAvi;
            if (request.CategoriaAvi != null) aviso.CategoriaAvi = request.CategoriaAvi;
            if (request.ActivoAvi.HasValue) aviso.ActivoAvi = request.ActivoAvi.Value;
            if (request.FechaIniAvi.HasValue) aviso.FechaIniAvi = request.FechaIniAvi.Value.ToUtc();
            if (request.FechaFinAvi.HasValue) aviso.FechaFinAvi = request.FechaFinAvi.ToUtc();

            var result = await _context.SaveChangesAsync() > 0;

            if (result)
            {
                await BitacoraHelper.RegistrarAccionAsync(_bitacoraService, idUsuario, "EDITAR_AVISO",
                    $"Editó el aviso: {aviso.TituloAvi}");
            }

            return result;
        }

        private async Task<AvisoResponse> MapToResponse(Aviso aviso)
        {
            var rol = await _context.Roles.FindAsync(aviso.IdRolesAvi);
            return new AvisoResponse
            {
                IdAvisos = aviso.IdAvisos,
                IdRolesAvi = aviso.IdRolesAvi,
                TituloAvi = aviso.TituloAvi,
                MensajeAvi = aviso.MensajeAvi,
                CategoriaAvi = aviso.CategoriaAvi,
                ActivoAvi = aviso.ActivoAvi,
                FechaIniAvi = aviso.FechaIniAvi.ToEcuadorTime(),
                FechaFinAvi = aviso.FechaFinAvi.ToEcuadorTime(),
                NombreRol = rol?.NombreRol
            };
        }

        // Sobrecargas antiguas
        public async Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request)
            => await CreateAvisoAsync(request, "SYSTEM");

        public async Task<bool> DeleteAvisoAsync(string id)
            => await DeleteAvisoAsync(id, "SYSTEM");

        public async Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request)
            => await UpdateAvisoAsync(id, request, "SYSTEM");
    }
}