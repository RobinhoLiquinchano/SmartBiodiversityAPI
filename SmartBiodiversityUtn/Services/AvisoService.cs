using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Aviso;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class AvisoService(SmartBiodiversityUtnContext context) : IAvisoService
    {
        public async Task<AvisoResponse> CreateAvisoAsync(CreateAvisoRequest request)
        {
            // Buscar el rol de Administrador
            var adminRol = await context.Roles
                .FirstOrDefaultAsync(r => r.NombreRol == "Administrador");

            if (adminRol == null)
                throw new Exception("No se encontró el rol de Administrador.");

            var aviso = new Aviso
            {
                IdAvisos = "AVI-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                IdRolesAvi = adminRol.IdRoles,                    // ← Se asigna automáticamente
                TituloAvi = request.TituloAvi,
                MensajeAvi = request.MensajeAvi,
                CategoriaAvi = request.CategoriaAvi,
                ActivoAvi = true,
                FechaIniAvi = DateTime.UtcNow,
                FechaFinAvi = request.FechaFinAvi
            };

            context.Avisos.Add(aviso);
            await context.SaveChangesAsync();

            return await MapToResponse(aviso);
        }

        public async Task<bool> DeleteAvisoAsync(string id)
        {
            var aviso = await context.Avisos.FindAsync(id);
            if (aviso is null) return false;

            context.Avisos.Remove(aviso);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AvisoResponse>> GetAllAvisosAsync()
        {
            var avisos = await context.Avisos
                .Include(a => a.Rol)
                .ToListAsync();

            var responses = new List<AvisoResponse>();
            foreach (var aviso in avisos)
            {
                responses.Add(await MapToResponse(aviso));
            }
            return responses;
        }

        public async Task<AvisoResponse?> GetAvisoByIdAsync(string id)
        {
            var aviso = await context.Avisos
                .Include(a => a.Rol)
                .FirstOrDefaultAsync(a => a.IdAvisos == id);

            return aviso is null ? null : await MapToResponse(aviso);
        }

        public async Task<IEnumerable<AvisoResponse>> GetAvisosActivosAsync()
        {
            var hoy = DateTime.UtcNow;

            var avisos = await context.Avisos
                .Include(a => a.Rol)
                .Where(a => a.ActivoAvi &&
                            a.FechaIniAvi <= hoy &&
                            (a.FechaFinAvi == null || a.FechaFinAvi >= hoy))
                .ToListAsync();

            var responses = new List<AvisoResponse>();
            foreach (var aviso in avisos)
            {
                responses.Add(await MapToResponse(aviso));
            }
            return responses;
        }

        public async Task<bool> UpdateAvisoAsync(string id, UpdateAvisoRequest request)
        {
            var aviso = await context.Avisos.FindAsync(id);
            if (aviso is null) return false;

            if (request.TituloAvi != null) aviso.TituloAvi = request.TituloAvi;
            if (request.MensajeAvi != null) aviso.MensajeAvi = request.MensajeAvi;
            if (request.CategoriaAvi != null) aviso.CategoriaAvi = request.CategoriaAvi;
            if (request.ActivoAvi.HasValue) aviso.ActivoAvi = request.ActivoAvi.Value;
            if (request.FechaIniAvi.HasValue) aviso.FechaIniAvi = request.FechaIniAvi.Value;
            if (request.FechaFinAvi.HasValue) aviso.FechaFinAvi = request.FechaFinAvi;

            await context.SaveChangesAsync();
            return true;
        }

        private async Task<AvisoResponse> MapToResponse(Aviso aviso)
        {
            var rol = await context.Roles.FindAsync(aviso.IdRolesAvi);

            return new AvisoResponse
            {
                IdAvisos = aviso.IdAvisos,
                IdRolesAvi = aviso.IdRolesAvi,
                TituloAvi = aviso.TituloAvi,
                MensajeAvi = aviso.MensajeAvi,
                CategoriaAvi = aviso.CategoriaAvi,
                ActivoAvi = aviso.ActivoAvi,
                FechaIniAvi = aviso.FechaIniAvi,
                FechaFinAvi = aviso.FechaFinAvi,
                NombreRol = rol?.NombreRol
            };
        }
    }
}
